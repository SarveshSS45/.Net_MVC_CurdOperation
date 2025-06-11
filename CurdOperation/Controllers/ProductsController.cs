using CurdOperation.Models;
using CurdOperation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CurdOperation.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var products = await context.Products
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductDto productDto)
        {
            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("Image", "The Image file is required");
            }

            if (!ModelState.IsValid)
            {
                return View(productDto);
            }

            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssff") + Path.GetExtension(productDto.ImageFile!.FileName);
            string imageFullPath = Path.Combine(environment.WebRootPath, "products", newFileName);

            using (var stream = new FileStream(imageFullPath, FileMode.Create))
            {
                await productDto.ImageFile.CopyToAsync(stream);
            }

            Product product = new Product
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                Created = DateTime.Now
            };

            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> 
            Edit(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ProductDto dto = new ProductDto
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description
            };

            ViewBag.ExistingImage = product.ImageFileName;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductDto productDto)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ExistingImage = product.ImageFileName;
                return View(productDto);
            }

            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;

            if (productDto.ImageFile != null)
            {
                string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssff") + Path.GetExtension(productDto.ImageFile.FileName);
                string imagePath = Path.Combine(environment.WebRootPath, "products", newFileName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await productDto.ImageFile.CopyToAsync(stream);
                }

                product.ImageFileName = newFileName;
            }

            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id > 0)
            {
                var product = await context.Products.FindAsync(id);

                if (product != null)
                {
                    return View(product);
                }

                return NotFound();
            }

            return BadRequest(); 
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var imagePath = Path.Combine(environment.WebRootPath, "products", product.ImageFileName);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            context.Products.Remove(product);
            await context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
