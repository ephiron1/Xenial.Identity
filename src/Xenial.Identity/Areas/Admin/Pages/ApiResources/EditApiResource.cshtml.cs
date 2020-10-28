﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Data;

namespace Xenial.Identity.Areas.Admin.Pages.ApiResources
{
    public class EditApiResourceModel : PageModel
    {
        private readonly UserManager<XenialIdentityUser> userManager;

        public EditApiResourceModel(UserManager<XenialIdentityUser> userManager)
            => this.userManager = userManager;

        public class ApiResourceInputModel
        {
            [Required]
            public string UserName { get; set; }
        }


        [Required, BindProperty]
        public ApiResourceInputModel Input { get; set; }

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string id)
        {
            if (Input == null)
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    StatusMessage = "Error: Cannot find api resource";
                    return Page();
                }
                if (user != null)
                {
                    Input = new ApiResourceInputModel
                    {
                        UserName = user.UserName
                    };
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPost([FromRoute] string id)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    StatusMessage = "Error: Cannot find api resource";
                    return Page();
                }
                var result = await userManager.SetUserNameAsync(user, Input.UserName);
                if (result.Succeeded)
                {
                    var updateResult = await userManager.UpdateAsync(user);

                    if (updateResult.Succeeded)
                    {
                        return Redirect("/Admin/ApiResources");
                    }
                    else
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError(error.Description, error.Description);
                        }
                        StatusMessage = "Error saving api resource";
                        return Page();
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Description, error.Description);
                    }
                    StatusMessage = "Error setting api resource name";
                    return Page();
                }
            }

            StatusMessage = "Error: Check Validation";

            return Page();
        }
    }
}