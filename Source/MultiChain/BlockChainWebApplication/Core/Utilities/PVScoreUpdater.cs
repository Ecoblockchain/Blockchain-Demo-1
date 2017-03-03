using BlockChainWebApplication.Core.Entities.SAPI;
using BlockChainWebApplication.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BlockChainWebApplication.Core.Utilities
{
    [DisallowConcurrentExecution]
    public class PVScoreUpdater : IJob
    {
        private static bool ShouldIncrement;

        public async void Execute(IJobExecutionContext context)
        {
            if (!AppManager.IsPVScoreUpdatorRunning)
            {
                AppManager.IsPVScoreUpdatorRunning = true;

                try
                {
                    using (var db = new DatabaseEntities())
                    {
                        List<User> _users = await db.Users.Where(u => u.IsDataActive).ToListAsync();

                        Random random = new Random();
                        double _factor = random.NextDouble() * (2 - 0);

                        foreach (User _user in _users)
                        {
                            SAPIResponse _response = await AppManager.GetPV(_user.Email);

                            if (_response.data != null)
                            {
                                if (_user.PV == null)
                                {
                                    _user.PV = (decimal)_response.data.pv;
                                    await db.SaveChangesAsync();
                                }

                                if (ShouldIncrement)
                                {
                                    if ((_response.data.pv + 2) > (double)_user.PV)
                                    {
                                        _user.PV = _user.PV + (decimal)_factor;
                                        _user.PVUpdaterStatus = true;
                                    }
                                    else
                                    {
                                        _user.PV = (decimal)_response.data.pv;
                                        _user.PVUpdaterStatus = null;
                                    }

                                }
                                else
                                {
                                    if ((_response.data.pv - 2) < (double)_user.PV)
                                    {
                                        _user.PV = _user.PV - (decimal)_factor;
                                        _user.PVUpdaterStatus = false;

                                    }
                                    else
                                    {
                                        _user.PV = (decimal)_response.data.pv;
                                        _user.PVUpdaterStatus = null;
                                    }
                                }

                                _user.LastUpdatedOn = DateTime.Now;

                                await db.SaveChangesAsync();
                            }

                        }

                        ShouldIncrement = ShouldIncrement ? false : true;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionHandler.Handle(ex);
                }
                finally
                {
                    AppManager.IsPVScoreUpdatorRunning = false;
                }
            }
        }
    }
}