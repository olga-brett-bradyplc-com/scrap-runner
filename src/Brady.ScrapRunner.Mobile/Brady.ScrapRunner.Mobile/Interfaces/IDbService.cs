﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface IDbService
    {
        Task RefreshAll();
        Task RefreshTable<T>() where T : class;
    }
}
