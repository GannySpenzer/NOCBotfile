﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PODOverrideReload
{
    class POOverrideReloadBO
    {
    }

    public class Item
    {
        public string tableName { get; set; }
        public int count { get; set; }
        public List<string> columnNames { get; set; }
        public List<List<string>> rows { get; set; }
    }

    public class Link
    {
        public string rel { get; set; }
        public string href { get; set; }
        public string mediaType { get; set; }
    }

    public class RootObject
    {
        public List<Item> items { get; set; }
        public List<Link> links { get; set; }
    }
}
