using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChessBucket.Data;
using ChessBucket.Models;

namespace ChessBucket.Controllers.Api
{
    [Produces("application/json")]
    [Route("api/GameTags")]
    public class GameTagsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GameTagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/gametags/game/5
        [HttpGet("game/{gameId}")]
        public IActionResult GetGameTagByGameId([FromRoute] int gameId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Tag> gameTag = _context.GameTags.Include(x => x.Tag).Where(m => m.Game.Id == gameId).Select(x => x.Tag).ToList();

            if (!gameTag.Any())
            {
                return NotFound();
            }

            return Ok(gameTag);
        }

        // GET: api/gametags/tag/5
        [HttpGet("tag/{tagId}")]
        public IActionResult GetGameTagByTagId([FromRoute] int tagId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<GameTag> gameTag = _context.GameTags.Include(x => x.Game).Include(x => x.Tag).Where(m => m.Tag.Id == tagId).ToList();

            if (!gameTag.Any())
            {
                return NotFound();
            }

            return Ok(gameTag);
        }

        // PUT: api/GameTags/5
        [HttpPut("{id}")]
        public IActionResult PutGameTag([FromRoute] int id, [FromBody] GameTag gameTag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != gameTag.Id)
            {
                return BadRequest();
            }

            _context.Entry(gameTag).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GameTagExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/GameTags
        [HttpPost]
        public IActionResult PostGameTag([FromBody] GameTag gameTag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.GameTags.Add(gameTag);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (GameTagExists(gameTag.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetGameTag", new { id = gameTag.Id }, gameTag);
        }

        // DELETE: api/GameTags/5
        [HttpDelete("{id}")]
        public IActionResult DeleteGameTag([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            GameTag gameTag = _context.GameTags.SingleOrDefault(m => m.Id == id);
            if (gameTag == null)
            {
                return NotFound();
            }

            _context.GameTags.Remove(gameTag);
            _context.SaveChanges();

            return Ok(gameTag);
        }

        private bool GameTagExists(int id)
        {
            return _context.GameTags.Any(e => e.Id == id);
        }
    }
}