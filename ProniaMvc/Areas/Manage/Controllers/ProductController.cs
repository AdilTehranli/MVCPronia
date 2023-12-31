﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCPronia.Services.Interfaces;
using MVCPronia.ViewModels.ProductVMs;
using ProniaMvc.DataAccess;
using ProniaMvc.Extentions;
using ProniaMvc.ExtentionsServices.Interfaces;
using ProniaMvc.Services.Interfaces;
using ProniaMvc.ViewModels.ProductVMs;

namespace ProniaMvc.Areas.Manage.Controllers;
[Area("Manage")]
public class ProductController : Controller
{
    readonly IFileService _fileService;
    readonly ProniaDbContext _context;
    readonly IProductService _service;
    readonly ICategoryService _category;

    public ProductController(IProductService service, ProniaDbContext context, ICategoryService category)
    {
        _service = service;
        _context = context;
        _category = category;
    }
    public async Task<IActionResult> Index()
    {
      
        return View(await _service.GetTable.Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category).ToListAsync());
    }
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(_category.GetTable, "Id", "Name");

        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductVM vm)
    {
        if(vm.MainImageFile != null)
        {
            if (!vm.MainImageFile.IsTypeValid("image"))
            {
                ModelState.AddModelError("MainImageFile", "Wrong file type");
            }
            if (!vm.MainImageFile.IsSizeValid(2))
            {
                ModelState.AddModelError("MainImageFile", "file max size is 2 mb");
            }
        }
        if (vm.HoverImageFile != null)
        {
            if (!vm.HoverImageFile.IsTypeValid("image"))
            {
                ModelState.AddModelError("HoverImageFile", "Wrong file type");
            }
            if (!vm.HoverImageFile.IsSizeValid(2))
            {
                ModelState.AddModelError("HoverImageFile", "file max size is 2 mb");
            }       
        }
        if(vm.ImageFiles != null)
        {
            foreach (var item in vm.ImageFiles)
            {
                if (!item.IsTypeValid("image"))
                {
                    ModelState.AddModelError("ImageFiles", "Wrong file type");
                }
                if (!item.IsSizeValid(2))
                {
                    ModelState.AddModelError("ImageFiles", "file max size is 2 mb");
                }
            }
        }
        if (!ModelState.IsValid) return View();
        await _service.Create(vm);
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Delete(int? id)
    {
        await _service.Delete(id);
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> ChangeStatus(int? id)
    {
        await _service.SoftDelete(id);
        TempData["IsDeleted"] = true;
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Update(int? id)
    {
        if(id == null ||  id <= 0) return BadRequest();
        var entity = await _service.GetTable
            .Include(p => p.ProductImages).Include(p=>p.ProductCategories).SingleOrDefaultAsync(p => p.Id == id);
        if (entity == null) return BadRequest();
        ViewBag.Categories = new SelectList(_category.GetTable, "Id", "Name");
        UpdateProductGETVM vm = new UpdateProductGETVM
        {
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            StockCount = entity.StockCount,
            Rating = entity.Rating,
            Discount = entity.Discount,
            MainImage = entity.MainImage,
            HoverImage = entity.HoverImage,
            ProductImages = entity.ProductImages,
            ProductCategoryIds = entity.ProductCategories.Select(p => p.CategoryId).ToList()
        };
        return View(entity);
    }
    [HttpPost]
    public async Task<IActionResult> Update(int? id,UpdateProductGETVM vm)
    {
        if (id == null || id <= 0) return BadRequest();
        var entity = await _service.GetById(id); 
        if (entity == null) return BadRequest();
        UpdateProductVM uvm = new UpdateProductVM
        {
            Name = vm.Name,
            Description = vm.Description,
            Price = vm.Price,
            StockCount = vm.StockCount,
            Rating = vm.Rating,
            Discount = vm.Discount,
            HoverImage = vm.HoverImageFile,
            MainImage = vm.MainImageFile,
            ProductImageFiles = vm.ProductImageFiles,
            CategoryIds=vm.ProductCategoryIds

        };
        await _service.Update(id, uvm);
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> DeleteImage(int id)
    {
        if (id == null || id <= 0) return BadRequest();
        await _service.DeleteImage(id);
        return Ok();
    

    }
}

