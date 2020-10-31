﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Xenial.Identity.Xpo.Storage.Models;

using IdentityServer4.Models;

namespace Xenial.Identity.Areas.Admin.Pages.Clients
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class IconAttribute : Attribute
    {
        public string Icon { get; }
        public IconAttribute(string icon)
            => Icon = icon;
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class HeaderAttribute : Attribute
    {
        public string Header { get; }
        public HeaderAttribute(string header)
            => Header = header;
    }

    public static class EnumHelper
    {
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }
    }

    public class AddClientModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<AddClientModel> logger;
        public AddClientModel(UnitOfWork unitOfWork, ILogger<AddClientModel> logger)
            => (this.unitOfWork, this.logger) = (unitOfWork, logger);

        public class ClientInputModel
        {
            [Required]
            public string ClientId { get; set; }
            [Required]
            public string ClientName { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }

            public ClientTypes ClientType { get; set; }

            public enum ClientTypes
            {
                [Icon("fas fa-file")]
                [Header("Empty")]
                [Description("Manual configuration")]
                Empty = 0,
                [Icon("fas fa-file-code")]
                [Header("WebApplication")]
                [Description("ServerSide (Auth Code Flow with PKCE)")]
                Web = 1,
                [Icon("fas fa-laptop")]
                [Header("SPA")]
                [Description("Javascript (Auth Code Flow with PKCE)")]
                Spa = 2,
                [Icon("fas fa-mobile")]
                [Header("Native Application")]
                [Description("Mobile/Desktop (Auth Code Flow with PKCE)")]
                Native = 3,
                [Icon("fas fa-server")]
                [Header("Machine/Robot")]
                [Description("Client Credentials flow")]
                Machine = 4,
                [Icon("fas fa-tv")]
                [Header("Device flow")]
                [Description("TV and Limited-Input Device Application")]
                Device = 5
            }
        }

        public IEnumerable<(ClientInputModel.ClientTypes, string header, string description, string icon)> GetClientTypes()
        {
            foreach (ClientInputModel.ClientTypes value in Enum.GetValues(typeof(ClientInputModel.ClientTypes)))
            {
                var header = value.GetAttributeOfType<HeaderAttribute>().Header;
                var description = value.GetAttributeOfType<DescriptionAttribute>().Description;
                var icon = value.GetAttributeOfType<IconAttribute>().Icon;
                yield return (value, header, description, icon);
            }
        }

        internal class ClientMappingConfiguration : Profile
        {
            public ClientMappingConfiguration()
                => CreateMap<ClientInputModel, XpoClient>()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ClientMappingConfiguration>())
                .CreateMapper();

        [Required, BindProperty]
        public ClientInputModel Input { get; set; } = new ClientInputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var client = Mapper.Map(Input, new XpoClient(unitOfWork));

                    PrepareClientTypeForNewClient(client);

                    await unitOfWork.SaveAsync(client);
                    await unitOfWork.CommitChangesAsync();
                    return Redirect("/Admin/Clients");
                }
                catch (ConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "Error saving Client with {ClientName}", Input?.ClientName);
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.ClientName)}", "Client name must be unique");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error saving Client with {ClientName}", Input?.ClientName);
                    StatusMessage = $"Error saving client: {ex.Message}";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }

        private void PrepareClientTypeForNewClient(XpoClient client)
        {
            switch (Input.ClientType)
            {
                case ClientInputModel.ClientTypes.Empty:
                    break;
                case ClientInputModel.ClientTypes.Web:
                    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.Code, client));
                    client.RequirePkce = true;
                    client.RequireClientSecret = true;
                    break;
                case ClientInputModel.ClientTypes.Spa:
                    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.Code, client));
                    client.RequirePkce = true;
                    client.RequireClientSecret = false;
                    break;
                case ClientInputModel.ClientTypes.Native:
                    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.Code, client));
                    client.RequirePkce = true;
                    client.RequireClientSecret = false;
                    break;
                case ClientInputModel.ClientTypes.Machine:
                    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.ClientCredentials, client));
                    break;
                case ClientInputModel.ClientTypes.Device:
                    client.AllowedGrantTypes.AddRange(CreateGrantTypes(GrantTypes.DeviceFlow, client));
                    client.RequireClientSecret = false;
                    client.AllowOfflineAccess = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            static IEnumerable<XpoClientGrantType> CreateGrantTypes(IEnumerable<string> grantTypes, XpoClient client)
                => grantTypes.Select(grant => new XpoClientGrantType(client.Session) { GrantType = grant });
        }
    }
}
