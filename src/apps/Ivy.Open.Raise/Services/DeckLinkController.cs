using Microsoft.AspNetCore.Mvc;
using Ivy.Open.Raise.Connections.Data;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Services;

[ApiController]
public class DeckLinkController(DataContextFactory factory, IBlobService blobService) : ControllerBase
{
    [HttpGet]
    [Route("links/{secret}")]
    public async Task<IActionResult> GetDeckLink(string secret, CancellationToken cancellationToken)
    {
        await using var db = factory.CreateDbContext();

        var deckLink = await db.DeckLinks
            .Include(dl => dl.Deck)
            .ThenInclude(d => d.DeckVersions)
            .FirstOrDefaultAsync(dl => dl.Secret == secret && dl.DeletedAt == null, cancellationToken);

        if (deckLink == null)
        {
            return NotFound(new { error = "Deck not found." });
        }

        var primaryVersion = deckLink.Deck.DeckVersions
            .FirstOrDefault(v => v is { IsPrimary: true, DeletedAt: null });

        if (primaryVersion == null)
        {
            return NotFound(new { error = "Deck not found." });
        }
        
        var deckLinkView = new DeckLinkView
        {
            Id = Guid.NewGuid(),
            DeckLinkId = deckLink.Id,
            ViewedAt = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };
        db.DeckLinkViews.Add(deckLinkView);
        await db.SaveChangesAsync(cancellationToken);

        var stream = await blobService.DownloadAsync(
            Constants.DeckBlobContainerName,
            primaryVersion.BlobName,
            cancellationToken);

        return File(stream, primaryVersion.ContentType, primaryVersion.FileName);
    }
}
