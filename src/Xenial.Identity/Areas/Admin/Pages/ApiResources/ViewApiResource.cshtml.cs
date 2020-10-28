﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Data;

namespace Xenial.Identity.Areas.Admin.Pages.ApiResources
{
    public class ViewApiResourceModel : PageModel
    {
        private readonly UserManager<XenialIdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public ViewApiResourceModel(
            UserManager<XenialIdentityUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
            => (this.userManager, this.roleManager) = (userManager, roleManager);

        public class ApiResourceOutputModel
        {
            public string Id { get; set; }

            public string UserName { get; set; }

            public List<ClaimModel> Claims { get; set; } = new List<ClaimModel>();
            public List<RoleModel> Roles { get; set; } = new List<RoleModel>();

            public class ClaimModel
            {
                public string Type { get; set; }
                public string Value { get; set; }
                public string Issuer { get; set; }
            }

            public class RoleModel
            {
                public string Id { get; set; }
                public string Name { get; set; }
            }
        }

        public ApiResourceOutputModel Output { get; set; } = new ApiResourceOutputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                StatusMessage = "Error: Cannot find api resource";
                return Page();
            }

            var claims = await userManager.GetClaimsAsync(user);
            var roleNames = await userManager.GetRolesAsync(user);

            var roleTasks = roleNames.Select(r => roleManager.NormalizeKey(r)).Select(async r => await roleManager.FindByNameAsync(r));

            var roles = await Task.WhenAll(roleTasks);

            Output = new ApiResourceOutputModel
            {
                UserName = user.UserName,
                Id = user.Id,
                Claims = claims.Select(c => new ApiResourceOutputModel.ClaimModel
                {
                    Type = c.Type,
                    Value = c.Value,
                    Issuer = c.Issuer,
                }).ToList(),
                Roles = roles.Select(r => new ApiResourceOutputModel.RoleModel
                {
                    Id = r.Id,
                    Name = r.Name,
                }).ToList()
            };

            return Page();
        }
    }
}