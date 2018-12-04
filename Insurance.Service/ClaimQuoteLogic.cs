using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Service
{
    public class ClaimQuoteLogic
    {
        public decimal ExcessesAmount { get; set; }
        public decimal isDriver { get; set; }
        public decimal IsPartial { get; set; }
        public decimal IsLossInZimba { get; set; }
        public decimal IsStolen { get; set; }
        public decimal Islicensedless { get; set; }
        public decimal? FinalAmount { get; set; }
        public decimal CheckPartialloss { get; set; }
        public decimal CheckLossInZimbabwe { get; set; }
        public decimal CheckDriverIsUnder21 { get; set; }
        public decimal CheckIsStolen { get; set; }

        public decimal CheckIslicensedless60months { get; set; }

        public decimal CheckIsDriver25 { get; set; }

        public decimal CheckIsSoundSystem { get; set; }

        public ClaimQuoteLogic CalculateClaimPremium(decimal sumInsured, int? IsPartialLoss, int? IsLossInZimbabwe, int? IsStolen, int? Islicensedless60months, int? DriverIsUnder21, Boolean PrivateCar, Boolean CommercialCar, int? IsDriver25, int? IsSoundSystem, decimal? TotalAmount, decimal? FinalAmountToPaid)
        {

            var ClaimSettingdetail = InsuranceContext.ClaimSettings.All(where: $"IsActive = 'True' or IsActive is Null ");
            //var Private = ClaimSettingdetail.Where(x => x.KeyName == "PrivateDriverUnder21");
            decimal DriverIsUnder = 0.0m;
            decimal licensedless = 0.0m;
            decimal PartialLoss = 0.0m;
            decimal totalloss = 0.0m;
            decimal Isstolen = 0.0m;
            decimal IslossInzimbabwe = 0.0m;
            decimal IsDriverunder25 = 0.0m;
            decimal IsSoundsystem = 0.0m;
            this.ExcessesAmount = 0.0m;
            // Boolean Parameter = false;
            decimal PDriverunder21amount = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "PrivateDriverUnder21").Select(x => x.Value).FirstOrDefault());
            decimal PLicensedless60monthsamount = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "PrivateLicenceLess60Month").Select(x => x.Value).FirstOrDefault());
            decimal PPartialLossamount = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "PrivatePartialLoss").Select(x => x.Value).FirstOrDefault());
            decimal PStolenamount = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "PrivateStolen/Accessories").Select(x => x.Value).FirstOrDefault());
            decimal POutSideOfZimba = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "PrivateOutSideOfZimba").Select(x => x.Value).FirstOrDefault());
            decimal PCarTotalLoss = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "PrivateCarTotalLoss").Select(x => x.Value).FirstOrDefault());
            decimal PSoundSystem = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "PrivateSoundSystem").Select(x => x.Value).FirstOrDefault());
            //Commercial Car 
            decimal CTotalLoss = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "CommercialTotalLoss").Select(x => x.Value).FirstOrDefault());
            decimal CPartialLoss = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "CommercialPartialLoss").Select(x => x.Value).FirstOrDefault());
            decimal COutSideZimba = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "CommercialOutSideZimba").Select(x => x.Value).FirstOrDefault());
            decimal CDriver25 = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "CommercialDriver25").Select(x => x.Value).FirstOrDefault());
            decimal CLicenceLess60Month = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "CommercialLicenceLess60Month").Select(x => x.Value).FirstOrDefault());
            decimal CStolenAccessories = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "CommercialStolen/Accessories").Select(x => x.Value).FirstOrDefault());
            decimal CSpundSystem = Convert.ToInt32(ClaimSettingdetail.Where(x => x.KeyName == "CommercialSoundSystem").Select(x => x.Value).FirstOrDefault());
            //this.ExcessesAmount = sumInsured;

            if (PrivateCar || CommercialCar)
            {


                if (PrivateCar)
                {

                    if (IsPartialLoss == null && IsLossInZimbabwe == null && IsStolen == null && Islicensedless60months == null && DriverIsUnder21 == null && PrivateCar == true && CommercialCar == false && IsDriver25 == null && IsSoundSystem == null)
                    {

                        sumInsured = 0.0m;
                    }

                    else
                    {
                        if (DriverIsUnder21 == 1)
                        {
                            DriverIsUnder = (sumInsured * PDriverunder21amount) / 100;
                            this.CheckDriverIsUnder21 = sumInsured + DriverIsUnder;
                        }
                        if (Islicensedless60months == 1)
                        {
                            licensedless = (sumInsured * PLicensedless60monthsamount) / 100;
                            this.CheckIslicensedless60months = sumInsured + licensedless;

                        }

                        if (IsPartialLoss == 1)
                        {
                            PartialLoss = (sumInsured * PPartialLossamount) / 100;
                            this.CheckPartialloss = sumInsured + PartialLoss;
                        }
                        else if (IsPartialLoss == 0)
                        {
                            totalloss = (sumInsured * PCarTotalLoss) / 100;
                            this.CheckPartialloss = sumInsured + totalloss;
                        }
                        if (IsStolen == 1)
                        {
                            Isstolen = (sumInsured * PStolenamount) / 100;
                            this.CheckIsStolen = sumInsured + Isstolen;
                        }
                        else if (IsStolen == 0)
                        {
                            Isstolen = (sumInsured * 1) / 100;
                            this.CheckIsStolen = sumInsured + Isstolen;
                        }
                        if (IsLossInZimbabwe == 1)
                        {
                            IslossInzimbabwe = (sumInsured * POutSideOfZimba) / 100;
                            this.CheckLossInZimbabwe = sumInsured + IslossInzimbabwe;
                        }
                       
                        if (IsSoundSystem == 1)
                        {
                            IsSoundsystem = (sumInsured * PSoundSystem) / 100;
                            this.CheckIsSoundSystem = sumInsured + IsSoundsystem;
                        }
                        else if (IsSoundSystem == 0)
                        {
                            IsSoundsystem = (sumInsured * 1) / 100;
                            this.CheckIsSoundSystem = sumInsured + IsSoundsystem;
                        }
                    }
                    if (DriverIsUnder21 == 1 || IsPartialLoss == 1 || IsLossInZimbabwe == 1|| IsPartialLoss == 0 || Islicensedless60months==1 || IsStolen==1 || IsStolen==0 || IsSoundSystem==0 || IsSoundSystem==1)
                    {

                        this.ExcessesAmount = this.CheckPartialloss + this.CheckLossInZimbabwe + this.CheckDriverIsUnder21 + this.CheckIslicensedless60months + this.CheckIsStolen+ this.CheckIsSoundSystem ;

                        this.FinalAmount = TotalAmount - this.ExcessesAmount;
                    }
                    else
                    {
                        this.ExcessesAmount = sumInsured;
                        this.FinalAmount = TotalAmount;
                    }


                }
                else if (CommercialCar)
                {

                    if (IsPartialLoss == null && IsLossInZimbabwe == null && IsStolen == null && Islicensedless60months == null && DriverIsUnder21 == null && PrivateCar == false && CommercialCar == true && IsDriver25 == null && IsSoundSystem == null)
                    {

                        sumInsured = 0.0m;
                    }
                    else
                    {
                        if (IsDriver25 == 1)
                        {
                            IsDriverunder25 = (sumInsured * CDriver25) / 100;
                            this.CheckIsDriver25 = sumInsured + IsDriverunder25;
                        }

                        if (Islicensedless60months == 1)
                        {
                            licensedless = (sumInsured * CLicenceLess60Month) / 100;
                            this.CheckIslicensedless60months = sumInsured + licensedless;
                        }

                        if (IsPartialLoss == 1)
                        {
                            PartialLoss = (sumInsured * CPartialLoss) / 100;
                            this.CheckPartialloss = sumInsured + PartialLoss;
                        }
                        else if (IsPartialLoss == 0)
                        {
                            totalloss = (sumInsured * CTotalLoss) / 100;
                            this.CheckPartialloss = sumInsured + totalloss;
                        }
                        if (IsStolen == 1)
                        {
                            Isstolen = (sumInsured * CStolenAccessories) / 100;
                            this.CheckIsStolen = sumInsured + Isstolen;
                        }
                        else if (IsStolen == 0)
                        {
                            Isstolen = (sumInsured * 1) / 100;
                            this.CheckIsStolen = sumInsured + Isstolen;
                        }
                        if (IsLossInZimbabwe == 1)
                        {
                            IslossInzimbabwe = (sumInsured * COutSideZimba) / 100;
                            this.CheckLossInZimbabwe =Convert.ToDecimal(sumInsured + IsLossInZimbabwe);
                        }
                        if (IsSoundSystem == 1)
                        {
                            IsSoundsystem = (sumInsured * CSpundSystem) / 100;
                            this.CheckIsSoundSystem = (sumInsured + IsSoundsystem);
                        }
                        else if (IsSoundSystem == 0)
                        {
                            IsSoundsystem = (sumInsured * 1) / 100;
                            this.CheckIsSoundSystem = (sumInsured + IsSoundsystem);
                        }
                    }


                    if (IsDriver25 == 1 || IsPartialLoss == 1 || IsLossInZimbabwe == 1 || IsPartialLoss == 0 || Islicensedless60months == 1 || IsStolen == 1 || IsStolen == 0 || IsSoundSystem == 0 || IsSoundSystem == 1)
                    {

                        this.ExcessesAmount = this.CheckPartialloss + this.CheckLossInZimbabwe + this.CheckIsDriver25 + this.CheckIslicensedless60months + this.CheckIsStolen + this.CheckIsSoundSystem;
                        this.FinalAmount = TotalAmount - this.ExcessesAmount;
                    }
                    else
                    {
                        this.ExcessesAmount = sumInsured;
                        this.FinalAmount = TotalAmount;
                    }
                }

            }
            else
            {
                this.ExcessesAmount = 0.0m;
                this.FinalAmount = TotalAmount;
            }
          
            //if (PrivateCar || CommercialCar)
            //{
            //    if (PrivateCar)
            //    {
            //        if (DriverIsUnder21 == 1)
            //        {
            //            this.FinalAmount = TotalAmount - ExcessesAmount;

            //        }
            //        else if (DriverIsUnder21 == 0)
            //        {
            //            this.FinalAmount = TotalAmount;
            //        }

            //        else
            //        {
            //            this.FinalAmount = TotalAmount;
            //        }

            //        if (Islicensedless60months == 1)
            //        {
            //            this.FinalAmount = TotalAmount - ExcessesAmount;
            //        }
            //        else if (Islicensedless60months == 0)
            //        {
            //            this.FinalAmount = TotalAmount;
            //        }

            //        if (IsSoundSystem == 1)
            //        {
            //            this.FinalAmount = TotalAmount - ExcessesAmount;
            //        }
            //        else if (IsSoundSystem == 0)
            //        {
            //            this.FinalAmount = TotalAmount - ExcessesAmount;

            //        }
            //        if (IsPartialLoss == 1)
            //        {
            //            this.FinalAmount = TotalAmount - ExcessesAmount;
            //        }
            //        else if(IsPartialLoss == 0)
            //        {
            //            this.FinalAmount = TotalAmount - ExcessesAmount;
            //        }
            //        if (IsStolen==1)
            //        {
            //            this.FinalAmount = TotalAmount - ExcessesAmount;
            //        }
            //        else if (IsStolen==0)
            //        {
            //            this.FinalAmount = TotalAmount - ExcessesAmount;
            //        }
            //        if (true)
            //        {

            //        }
            //    }
            //    if (CommercialCar)
            //    {
            //        if (IsDriver25 == 1)
            //        {
            //            this.FinalAmount = TotalAmount - ExcessesAmount;
            //        }
            //        else if (IsDriver25 == 0)
            //        {
            //            this.FinalAmount = TotalAmount;
            //        }
            //        else
            //        {
            //            this.FinalAmount = TotalAmount;
            //        }
            //        if (Islicensedless60months == 1)
            //        {
            //            this.FinalAmount = TotalAmount - ExcessesAmount;
            //        }
            //        else if (Islicensedless60months == 0)
            //        {
            //            this.FinalAmount = TotalAmount;
            //        }

            //        if (IsSoundSystem == 1)
            //        {
            //            this.FinalAmount = TotalAmount - ExcessesAmount;
            //        }
            //        else if (IsSoundSystem == 0)
            //        {
            //            this.FinalAmount = TotalAmount - ExcessesAmount;

            //        }
            //    }
            //}
            //else
            //{
            //    this.FinalAmount = TotalAmount;
            //}
            return this;
        }

    }
}



