﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFHelpers.ViewModelBase
{
    public interface IViewModelBase
    {
        event PropertyChangedEventHandler PropertyChanged;
    }
}
