﻿using MVCPronia.Models;
using Microsoft.EntityFrameworkCore;
using MVCPronia.Services.Interfaces;
using ProniaMvc.DataAccess;
using ProniaMvc.ExtentionsServices.Interfaces;
using ProniaMvc.Models;
using ProniaMvc.Services.Interfaces;
using ProniaMvc.ViewModels.ProductVMs;

namespace ProniaMvc.Services.Implements;

public class ProductService : IProductService
{
    private readonly ProniaDbContext _context;
    readonly IFileService _fileService;
    readonly ICategoryService _catservice;

    IQueryable<Product> IProductService.GetTable { get => _context.Set<Product>(); }

    public ProductService(ProniaDbContext context, IFileService fileService, ICategoryService catservice)
    {
        _context = context;
        _fileService = fileService;
        _catservice = catservice;
    }

    public async Task Create(CreateProductVM productVM)
    {
        if (productVM.CategoryIds.Count > 4)
            throw new Exception();
        if (!await _catservice.IsAllExist(productVM.CategoryIds))
            throw new Exception();
        List<ProductCategory> products = new List<ProductCategory>();
        foreach (var id in productVM.CategoryIds)
        {
            products.Add(new ProductCategory
            {
                CategoryId = id
            });
        }
        Product entity = new Product()
        {
            Name = productVM.Name,
            Description = productVM.Description,
            Discount = productVM.Discount,
            Price = productVM.Price,
            Rating = productVM.Rating,
            StockCount = productVM.StockCount,
            MainImage = await _fileService.UploadAsync(productVM.MainImageFile, Path.Combine(
                "assets", "imgs", "products")),
            ProductCategories = products
        };
        if (productVM.ImageFiles != null)
        {

            List<ProductImage> imgs = new();
            foreach (var item in productVM.ImageFiles)
            {
                string fileName = await _fileService.UploadAsync(item, Path.Combine(
                "assets", "imgs", "products"));
                imgs.Add(new ProductImage
                {
                    Name = fileName
                });
            }
            entity.ProductImages = imgs;
        }
        if (productVM.HoverImageFile != null)
            entity.HoverImage = await _fileService.UploadAsync(productVM.HoverImageFile, Path.Combine(
                "assets", "imgs", "products"));
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int? id)
    {
        var entity = await GetById(id);
        _context.Remove(entity);
        _fileService.Delete(entity.MainImage);
        if (entity.HoverImage != null)
        {
            _fileService.Delete(entity.HoverImage);
        }
        if (entity.ProductImages != null)
        {
            foreach (var item in entity.ProductImages)
            {

                _fileService.Delete(item.Name);
            }

        }
        await _context.SaveChangesAsync();
    }

    public async Task<List<Product>> GetAll(bool takeAll)
    {
        if (takeAll)
        {
            return await _context.Products.ToListAsync();
        }
        return await _context.Products.Where(p => p.IsDeleted == false).ToListAsync();
    }

    public async Task<Product> GetById(int? id, bool takeAll = false)
    {
        if (id == null || id < 1) throw new ArgumentException();

        Product entity;

        if (takeAll)
        {
            entity = await _context.Products.FindAsync(id);
        }
        else
        {
            entity = await _context.Products.SingleOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);
        }

        if (entity is null) throw new ArgumentNullException();

        return entity;
    }


    public async Task Update(int? id, UpdateProductVM productVM)
    {
        if (productVM.CategoryIds.Count > 4)
            throw new Exception();
        if (!await _catservice.IsAllExist(productVM.CategoryIds))
            throw new Exception();
        List<ProductCategory> products = new List<ProductCategory>();
        foreach (var cid in productVM.CategoryIds)
        {
            products.Add(new ProductCategory
            {
                CategoryId = cid
            });

            var entity = await _context.Products.Include(p => p.ProductCategories).SingleOrDefaultAsync(p => p.Id == id);
            entity.Name = productVM.Name;
            entity.StockCount = productVM.StockCount;
            entity.Price = productVM.Price;
            entity.Description = productVM.Description;
            entity.Rating = productVM.Rating;
            entity.Discount = productVM.Discount;
            entity.ProductCategories = products;
            if (productVM.MainImage != null)
            {
                _fileService.Delete(entity.MainImage);
                entity.MainImage = await _fileService.UploadAsync(productVM.MainImage,
                    Path.Combine("assets", "imgs", "products"));
            }
            if (productVM.HoverImage != null)
            {
                if (entity.HoverImage != null)
                {

                    _fileService?.Delete(entity.HoverImage);
                }
                entity.HoverImage = await _fileService.UploadAsync(productVM.HoverImage,
                   Path.Combine("assets", "imgs", "products"));

            }
            if (productVM.ProductImageFiles != null)
            {
                if (entity.ProductImages == null) entity.ProductImages = new List<ProductImage>();
                foreach (var item in productVM.ProductImageFiles)
                {
                    ProductImage img = new ProductImage()
                    {
                        Name = await _fileService.UploadAsync(productVM.HoverImage,
                   Path.Combine("assets", "imgs", "products"))
                    };
                    entity.ProductImages.Add(img);
                }
            }
            await _context.SaveChangesAsync();

        }

       
    }
    public async Task SoftDelete(int? id)
    {
        var entity = await GetById(id);
        entity.IsDeleted = !entity.IsDeleted;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteImage(int? id)
    {
        if (id == null || id <= 0) throw new ArgumentNullException();
        var entity = await _context.ProductImages.FindAsync(id);
        if (entity == null) throw new NullReferenceException();
        _fileService.Delete(entity.Name);
        _context.ProductImages.Remove(entity);
        await _context.SaveChangesAsync();
    }

}
