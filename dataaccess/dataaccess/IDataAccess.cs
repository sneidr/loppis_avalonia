﻿using DataAccess.Model;
using System.Threading.Tasks;

namespace DataAccess.DataAccess;

public interface IDataAccess
{
    Task WriteSale(Sale sale);
    Task RemoveSale(Sale sale);
}
