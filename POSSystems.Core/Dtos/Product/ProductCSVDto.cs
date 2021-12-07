using System;

namespace POSSystems.Core.Dtos.Product
{
    public class ProductCSVDto
    {
        public string ITEM_NUMBER;
        public string CATEGORY;
        public string DESCRIPTION;
        public string UPC;
        public string QTY;
        public string COST_PRICE;
        public string RETAIL_PRICE;
    }

    public class ProductCSVDto2
    {
        public string product_name;
        public string upc_code;
        public string cndcfi;
        public string category_description;
        public string form;
        public string strength;
        public string package_size;
        public string sales_price;
        public string measurement_name;
        public string manufacture_name;
        public string category_name;
        public string expire_date;

        public string ScanCode
        {
            get
            {
                if (upc_code?.Length < 10) return null;

                var scanCode = upc_code;

                switch (cndcfi)
                {
                    case "0": break;
                    case "1":
                        scanCode = scanCode?.Remove(0, 1);
                        break;

                    case "2":
                        scanCode = scanCode?.Remove(5, 1);
                        break;

                    case "3":
                        scanCode = scanCode?.Remove(9, 1);
                        break;

                    case "4":
                        scanCode = scanCode?.Remove(5, 1);
                        break;

                    case "5":
                        scanCode = scanCode?.Remove(scanCode.Length - 2, 1);
                        break;

                    case "6":
                        scanCode = scanCode?.Remove(scanCode.Length - 1, 1);
                        break;

                    case "7":
                        scanCode = scanCode?.Remove(0, 1);
                        break;

                    case "8":
                        scanCode = scanCode?.Remove(9, 1);
                        break;

                    default:
                        scanCode = null;
                        break;
                }

                return scanCode;
            }
        }
    }
}