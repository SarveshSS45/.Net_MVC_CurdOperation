using CurdOperation.Models;
using CurdOperation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

        public IActionResult Index()
        {
            var products = context.Products.OrderByDescending(p => p.Id).ToList();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDto productDto)
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

            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
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

            context.Products.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);
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
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);
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
                    productDto.ImageFile.CopyTo(stream);
                }

                product.ImageFileName = newFileName;
            }

            context.SaveChanges();
            return RedirectToAction("Index");
        }

        
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);
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
            context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}


//[HttpGet]  -->   async method syntax in the .net
//public async Teak <IActionResult> Create()
//{
//    ViewBag.Departments = await DbFunction.Departments.ToListAsync();
//    return View();  
//}