﻿using Gdp.Mdlp.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service.Extensibility
{
    public interface IProcessEvent
    {
        Task<bool> Execute(EventTaskState state);
    }
}
