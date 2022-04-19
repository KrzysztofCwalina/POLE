using Azure.Core.Pole.Tooling;
using System;

namespace Azure.Cooking.Receipes
{
    public class _CookingReceipesClient
    {
        public _CookingReceipe GetReceipe(ulong id) => default;

        //public CookingReceipe[] GetReceipes(CookingReceipeQuery query = default) => default;
        //public ulong AddReceipe(CookingReceipeSubmission receipe) => default;
    }

    public class _CookingReceipe
    {
        public string Tile { get; }
        public string ImageUri { get; }
        public CookingReceipeIngredient[] Ingredients { get; }
        public string Directions { get; }
        public CookingReceipeCategory[] Categories { get; }
        public ulong Id { get; }
    }

    public class CookingReceipeSubmission
    {
        public string Tile { get; set; }
        public byte[] ImageBytes { get; set; }
        public string ImageFileName { get; set; }
        public CookingReceipeIngredient[] Ingredients { get; set; }
        public string Directions { get; set; }
        public CookingReceipeCategory[] Categories { get; set; }
    }

    public struct CookingReceipeIngredient
    {
        public string Name { get; set; }
        public string Quantity { get; set; }
    }

    public enum CookingReceipeCategory
    {
        Appetizer,
        Soup,
        Main,
        Dessert,
    }

    //public class CookingReceipeQuery
    //{
    //    public string Query { get; set; }
    //    public string[] Ingredients { get; set; }
    //}
}
