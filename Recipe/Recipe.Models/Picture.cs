﻿using Recipe.Models.Common;

namespace Recipe.Models
{
    public class Picture : IPicture
    {
        public int PictureID { get; set; }
        public IRecipe Recipe { get; set; }
        public byte[] Image { get; set; }
    }
}
