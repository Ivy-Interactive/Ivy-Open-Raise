using Microsoft.AspNetCore.Mvc;
using Ivy.Open.Raise.Connections.Data;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Services;

[ApiController]
public class DeckLinkController(DataContextFactory factory, IBlobService blobService) : ControllerBase
{
    //todo: how can I get a route working like this?
    
    [HttpGet]
    [Route("decks/link/{secret}")]
    public async Task<IActionResult> GetDeckLink(string secret, CancellationToken cancellationToken)
    {
        // await using var db = factory.CreateDbContext();
        //
        // var deckLink = await db.DeckLinks
        //     .Include(dl => dl.Deck)
        //     .ThenInclude(d => d.DeckVersions)
        //     .FirstOrDefaultAsync(dl => dl.Secret == secret && dl.DeletedAt == null, cancellationToken);
        //
        // if (deckLink == null)
        // {
        //     return NotFound(new { error = "Deck link not found" });
        // }
        //
        // var primaryVersion = deckLink.Deck.DeckVersions
        //     .FirstOrDefault(v => v.IsPrimary && v.DeletedAt == null);
        //
        // if (primaryVersion == null)
        // {
        //     return NotFound(new { error = "No primary version found" });
        // }
        //
        // // Get pre-signed download URL
        // var downloadUrl = await blobService.GetDownloadUrlAsync(
        //     Constants.DeckBlobContainerName,
        //     primaryVersion.BlobName,
        //     TimeSpan.FromHours(1),
        //     cancellationToken);

        // return Ok(new
        // {
        //     deckTitle = deckLink.Deck.Title,
        //     versionName = primaryVersion.Name,
        //     fileName = primaryVersion.FileName,
        //     downloadUrl = downloadUrl
        // });

        return Ok("Hello");
    }
}
