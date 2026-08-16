using System.Security.Claims;
using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using WebApi.Extensions;

namespace WebApi.Endpoints;

public static class MetronomeSettingsEndpoints
{
    public static void MapMetronomeSettingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var protectedEndpoints = endpoints.MapGroup("api").MapProtectedGroup().RequireRateLimiting("IdentityLimiter");

        protectedEndpoints.MapGet("/check-auth", (HttpContext context) =>
        {
            //var userNickName = context.Items["UserNickName"]?.ToString();
            return Results.Ok();
        });

        protectedEndpoints.MapPost("/create-metronome-settings", async (IMetronomeSettingsService metronomeSettingsService, HttpContext httpContext,MetronomeSettingsCreateDto dto, CancellationToken cancellationToken) =>
        {
            var userId = httpContext.Items["UserId"]?.ToString();
            if(string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            var presetCount = await metronomeSettingsService.GetUserPresetsCountAsync(userId, cancellationToken);

            if (presetCount >= 3)
                return Results.Ok(BusinessLogic.DTOs.ApiResponseDto.Fail(
                    "Saving is currently limited to 3 presets. We will notify users by email when saving is expanded.",
                    errorCode: "PRESET_LIMIT_REACHED"));

            var savedSettings = await metronomeSettingsService.CreateSettingsAsync(dto, userId, cancellationToken);

            return Results.Ok(savedSettings);
        }).RequireRateLimiting("SettingsLimiter");

        protectedEndpoints.MapGet("/get-user-presets", async (IMetronomeSettingsService service, HttpContext httpContext, CancellationToken cancellationToken) =>
        {
            var userId = httpContext.Items["UserId"]?.ToString();
            if (!Guid.TryParse(httpContext.Items["UserId"]?.ToString(), out var guidId))
                return Results.Unauthorized();

            var settings = await service.GetSettingsByUserGuidIdAsync(guidId.ToString(), cancellationToken);

            return Results.Ok(settings);
        }).RequireRateLimiting("SettingsLimiter");

        protectedEndpoints.MapGet("/get-user-preset-by-id/{id:int}", async (IMetronomeSettingsService service, int id, CancellationToken cancellationToken) =>
        {
            var settings = await service.GetSettingsByIdAsync(id, cancellationToken);
            return settings is null ? Results.NotFound() : Results.Ok(settings);
        }).RequireRateLimiting("SettingsLimiter");

        protectedEndpoints.MapPut("/update-metronome-settings/{id:int}", async (IMetronomeSettingsService metronomeSettingsService, int id, MetronomeSettingsUpdateDto dto, CancellationToken cancellationToken) =>
        {
            var updatedSettings = await metronomeSettingsService.UpdateSettingsAsync(id,dto, cancellationToken);
            return Results.Ok(updatedSettings);     
        }).RequireRateLimiting("SettingsLimiter");

        protectedEndpoints.MapDelete("/delete-metronome-settings/{id:int}", async (IMetronomeSettingsService metronomeSettingsService, int id) =>
        {
            var isDeleted = await metronomeSettingsService.DeleteSettingsAsync(id);

            if (!isDeleted) return Results.NotFound($"Settings with id {id} not found.");

            return Results.Ok($"Settings with id {id} successfully deleted.");
        }).RequireRateLimiting("SettingsLimiter");

        endpoints.MapPost("api/create-collection-name", async (IMetronomeCollectNameService metronomeCollNameService, IMapper mapper, CollectionNameCreateDto collectionName) =>
        {
            await metronomeCollNameService.CreateCollectionNameAsync(collectionName);

            //var collectionNameReadDto = mapper.Map<CollectionNameReadDto>(collectionName);

            //return Results.Created($"/api/create-collection/{}{collectionName.Name}", collectionName);
            return Results.Ok("Metronome collection name created");
        }).RequireRateLimiting("SettingsLimiter");

        endpoints.MapGet("api/get-collection-name/{id}", async (IMetronomeCollectNameService metronomeCollNameService, int id) =>
        {
            var collectionName = await metronomeCollNameService.GetCollectionNameByIdAsync(id);
            return Results.Ok(collectionName);
        }).RequireRateLimiting("SettingsLimiter");

        endpoints.MapPut("api/update-collection-name", async (IMetronomeCollectNameService metronomeCollNameService, CollectionNameUpdateDto collectionName) =>
        {
            await metronomeCollNameService.UpdateCollectionNameAsync(collectionName);
            return Results.Ok("Metronome collection name updated");
        }).RequireRateLimiting("SettingsLimiter");

        endpoints.MapGet("api/get-all-collection-names", async (IMetronomeCollectNameService metronomeCollNameService) =>
        {
            var collectionNames = await metronomeCollNameService.GetAllCollectionNamesAsync();
            return Results.Ok(collectionNames);
        }).RequireRateLimiting("SettingsLimiter");

        endpoints.MapDelete("api/delete-collection-name/{id}", async (IMetronomeCollectNameService metronomeCollNameService, int id) =>
        {
            await metronomeCollNameService.DeleteCollectionNameAsync(id);
            return Results.Ok("Metronome collection name deleted");
        }).RequireRateLimiting("SettingsLimiter");
    }
}