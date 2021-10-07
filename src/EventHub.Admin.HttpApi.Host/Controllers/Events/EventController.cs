﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventHub.Admin.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.VirtualFileSystem;

namespace EventHub.Admin.Controllers.Events
{
    [RemoteService(Name = EventHubAdminRemoteServiceConsts.RemoteServiceName)]
    [Controller]
    [Area("eventhub-admin")]
    [ControllerName("Event")]
    [Route("/api/eventhub/admin/event")]
    public class EventController : AbpController, IEventAppService
    {
        private readonly IEventAppService _eventAppService;
        private readonly IVirtualFileProvider _virtualFileProvider;
        
        public EventController(IEventAppService eventAppService, IVirtualFileProvider virtualFileProvider)
        {
            _eventAppService = eventAppService;
            _virtualFileProvider = virtualFileProvider;
        }

        [HttpGet("{id}")]
        public Task<EventDetailDto> GetAsync(Guid id)
        {
            return _eventAppService.GetAsync(id);
        }

        [HttpGet("countries")]
        public Task<List<CountryLookupDto>> GetCountriesLookupAsync()
        {
            return _eventAppService.GetCountriesLookupAsync();
        }

        [HttpGet("cover-image/{id}")]
        [AllowAnonymous]
        public async Task<IRemoteStreamContent> GetCoverImageAsync(Guid id)
        {
            var remoteStreamContent = await _eventAppService.GetCoverImageAsync(id);
            if (remoteStreamContent is null)
            {
                var stream = _virtualFileProvider
                    .GetFileInfo("/Images/eh-event.png")
                    .CreateReadStream();
                
                remoteStreamContent = new RemoteStreamContent(stream)
                {
                    ContentType = "image/png"
                };
                
                await stream.FlushAsync();
            }
            
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.ContentType = remoteStreamContent.ContentType;

            return remoteStreamContent;
        }

        [HttpGet]
        public Task<PagedResultDto<EventInListDto>> GetListAsync(EventListFilterDto input)
        {
            return _eventAppService.GetListAsync(input);
        }

        [HttpPut]
        public Task UpdateAsync(Guid id, [FromForm] UpdateEventDto input)
        {
            return _eventAppService.UpdateAsync(id, input);
        }
    }
}
