﻿using System.Security.Claims;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

using MudBlazor;

using Xenial.Identity.Data;

namespace Xenial.Identity.Components.Admin;

public partial class UserClaimDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }

    [Parameter, EditorRequired]
    public Claim Claim { get; set; }

    [Parameter]
    public EventCallback<Claim> ClaimChanged { get; set; }

    [Parameter, EditorRequired]
    public XenialIdentityUser User { get; set; }

    [Parameter]
    public bool IsNewClaim { get; set; }

    private async Task Save()
    {

        var result = await SaveClaim(Claim, new Claim(type, value));

        async Task<IdentityResult> SaveClaim(Claim oldClaim, Claim newClaim)
        {
            if (IsNewClaim)
            {
                return await UserManager.AddClaimAsync(User, new Claim(type, value));
            }
            var result = await UserManager.RemoveClaimAsync(User, oldClaim);
            if (result.Succeeded)
            {
                return await UserManager.AddClaimAsync(User, newClaim);
            }
            return result;
        }

        if (result.Succeeded)
        {
            MudDialog.Close(true);
        }
        else
        {
            var errors = string.Join("\n", result.Errors.Select(e => $"<li>Code: {e.Code}: {e.Description}</li>"));

            Snackbar.Add($"""
                <ul>
                    <li>
                        There was an error when updating the claim!
                    </li>
                    <li>
                        <em>{Claim.Type}</em><br>
                    </li>
                    {errors}
                </ul>
                """, MudBlazor.Severity.Error);
        }
    }


    private async Task SetClaim(Claim newValue)
    {
        Claim = newValue;
        await ClaimChanged.InvokeAsync(newValue);
    }

    private ClaimEditMode Mode { get; set; }

    private string type;
    private string value;

    private void SetType(string type)
        => this.type = type;

    private void SetValue(string value)
        => this.value = value;

    private void SetMode(ClaimEditMode mode)
    {
        Mode = mode;
        if (mode == ClaimEditMode.Json && string.IsNullOrWhiteSpace(value))
        {
            value = """
            {
                "": ""
            }
            """;
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        value = Claim.Value;
        type = Claim.Type;
        if (value?.StartsWith("{") ?? false)
        {
            Mode = ClaimEditMode.Json;
        }
        //if (value?.StartsWith("<") ?? false)
        //{
        //    Mode = ClaimEditMode.Xml;
        //}
    }

    private void Cancel()
        => MudDialog?.Close();

    private enum ClaimEditMode
    {
        Text,
        MultiLine,
        Json,
        //Xml
    }
}
