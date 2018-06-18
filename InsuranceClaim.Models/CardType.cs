using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public enum CreditCardTypeType
    {
        Visa,
        MasterCard,
        Discover,
        Amex,
        Switch,
        Solo
    }
    public class CardType
    {
        private const string cardRegex = @"^(?:(?<Visa>4\\d{3})|
    (?< MasterCard > 5[1 - 5]\\d{2})|(?<Discover>6011)|(?<DinersClub>
    (?:3[68]\\d{2})|(?:30[0-5]\\d))|(?<Amex>3[47]\\d{2}))([ -]?)
    (?(DinersClub)(?:\\d{6}\\1\\d{4})|(?(Amex)(?:\\d{6}\\1\\d{5})
    |(?:\\d{4}\\1\\d{4}\\1\\d{4})))$";

        public static CreditCardTypeType? GetCardTypeFromNumber(string cardNum)
        {
            //Create new instance of Regex comparer with our
            //credit card regex patter
            Regex cardTest = new Regex(cardRegex);

            //Compare the supplied card number with the regex
            //pattern and get reference regex named groups
            GroupCollection gc = cardTest.Match(cardNum).Groups;

            //Compare each card type to the named groups to 
            //determine which card type the number matches
            if (gc[CreditCardTypeType.Amex.ToString()].Success)
            {
                return CreditCardTypeType.Amex;
            }
            else if (gc[CreditCardTypeType.MasterCard.ToString()].Success)
            {
                return CreditCardTypeType.MasterCard;
            }
            else if (gc[CreditCardTypeType.Visa.ToString()].Success)
            {
                return CreditCardTypeType.Visa;
            }
            else if (gc[CreditCardTypeType.Discover.ToString()].Success)
            {
                return CreditCardTypeType.Discover;
            }
            else
            {
                //Card type is not supported by our system, return null
                //(You can modify this code to support more (or less)
                // card types as it pertains to your application)
                return null;
            }
        }
    }
}
