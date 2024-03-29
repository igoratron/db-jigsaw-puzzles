﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Data;

namespace jigsaw.Model
{
    public class Table
    {
        public String Name
        {
            get;
            set;
        }
        public int Size
        {
            get;
            set;
        }
        public List<Table> ForeignKey
        {
            get;
            private set;
        }
        public List<Table> From
        {
            get;
            private set;
        }

        public List<String> Columns
        {
            get;
            set;
        }

        public DataTable DataTable
        {
            get;
            private set;
        }

        public Table(String name, String size)
        {
            new Table(name, int.Parse(size));
        }

        public Table(String name, int size)
        {
            ForeignKey = new List<Table>();
            From = new List<Table>();
            Columns = new List<string>();
            DataTable = new DataTable();
            Size = size;
            Name = name;
        }

        public override string ToString()
        {
            return String.Format("Table = {0}", Name);
        }
    }
}
