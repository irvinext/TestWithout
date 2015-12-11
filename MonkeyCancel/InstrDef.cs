using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;

namespace MonkeyCancel
{
    [DelimitedRecord(",")]
    public class InstrDef
    {
        public string Exchange, Product, ProdType, Contract, Alias, PriceModel;

        public InstrDef(string Exchange, string Product, string ProdType, string Contract, string Alias, string PriceModel)
        {
            this.Exchange = Exchange;
            this.Product = Product;
            this.ProdType = ProdType;
            this.Contract = Contract;
            this.Alias = Alias;
            this.PriceModel = PriceModel;
        }

        private InstrDef()
        {

        }
    }
}
