﻿using System.Collections.Generic;

using AutoMapper;

using IdentityServer4.Models;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Xpo.Storage.Mappers
{
    /// <summary>
    /// Defines entity/model mapping for identity resources.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class IdentityResourceMapperProfile : Profile
    {
        /// <summary>
        /// <see cref="IdentityResourceMapperProfile"/>
        /// </summary>
        public IdentityResourceMapperProfile()
        {
            CreateMap<XpoIdentityResourceProperty, KeyValuePair<string, string>>()
                .ReverseMap();

            CreateMap<XpoIdentityResource, IdentityResource>(MemberList.Destination)
                .ConstructUsing(src => new IdentityResource())
                .ReverseMap();

            CreateMap<XpoIdentityResourceClaim, string>()
               .ConstructUsing(x => x.Type)
               .ReverseMap()
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src));
        }
    }
}
