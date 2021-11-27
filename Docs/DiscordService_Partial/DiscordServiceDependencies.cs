using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;
using UmbracoDiscord.Core.Repositories;

private readonly ILogger<DiscordService> _logger;
private readonly IMemberService _memberService;
private readonly DiscordRoleRepository _discordRoleRepository;
private readonly IConfiguration _configuration;
private readonly IScopeProvider _scopeProvider;

public DiscordService(ILogger<DiscordService> logger,
	IMemberService memberService,
	DiscordRoleRepository discordRoleRepository,
	IConfiguration configuration,
	IScopeProvider scopeProvider)
{
	_logger = logger;
	_memberService = memberService;
	_discordRoleRepository = discordRoleRepository;
	_configuration = configuration;
	_scopeProvider = scopeProvider;
}