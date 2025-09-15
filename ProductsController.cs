using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Reflection;
using System.Text.Json.Serialization;
using Newtonsoft.Json;


[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly DashboardKpiContext _context;

    public ProductsController(DashboardKpiContext context)
    {
        _context = context; 
// 

    }

    [Authorize(Roles ="Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        
        var type = this.GetType();
        var method = type.GetMethod("Get");
        
        var auths = method?.GetCustomAttribute<AuthorizeAttribute>();
        Console.WriteLine($"Roles : {JsonConvert.SerializeObject(auths)}");  


        return await _context.Products.ToListAsync();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromBody] Product product)
    {
        if (!ModelState.IsValid) { return BadRequest(ModelState); }
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct([FromRoute] int id, [FromBody] Product product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != product.Id)
        {
            return BadRequest();
        }

        _context.Entry(product).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductExists(id))
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

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(product);
    }

private bool ProductExists(int id)
    {
        return _context.Products.Any(p => p.Id == id);
    }

}
