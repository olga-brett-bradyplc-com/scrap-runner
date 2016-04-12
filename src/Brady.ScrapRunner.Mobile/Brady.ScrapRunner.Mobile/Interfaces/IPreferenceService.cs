﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface IPreferenceService
    {
        Task UpdatePreferences(IEnumerable<Preference> preferences);
    }
}