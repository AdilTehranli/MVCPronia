﻿using System.ComponentModel.DataAnnotations;

namespace ProniaMvc.Models;

public class ProductImage : BaseEntity
{
    [Required]
    public string Name { get; set; }
    [Required]
    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
