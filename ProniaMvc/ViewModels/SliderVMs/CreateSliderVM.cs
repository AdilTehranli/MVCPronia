﻿using System.ComponentModel.DataAnnotations;

namespace ProniaMvc.ViewModels.SliderVMs;

public record CreateSliderVM
{
    [Required, MaxLength(50)]
    public string Title { get; set; }
    [Required, MaxLength(200)]
    public string Description { get; set; }
    [MaxLength(50)]
    public string? Offer { get; set; }
    [Required, MaxLength(50)]
    public string ButtonText { get; set; }
    [Required]
    public IFormFile ImageFile { get; set; }
}
