﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class BaseEntriesResponse<T>
        : BaseResponse
        where T : class
    {
        public T[] entries { get; set; }
        public long? total { get; set; }
    }
}
