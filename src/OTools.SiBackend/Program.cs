using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SPORTident;
using SPORTident.Communication;
using System.IO;
using System.Threading;

namespace OTools.SiBackend
{
    public class Program
    {
        private static bool _continue = true;

        public static void Main(string[] args)
        {
            Action action = Action.NotSet;

            if (args.Length == 0)
            {
                action = Action.ReadCards;
            }
            else
            {
                string inp = args[0];

                switch (inp)
                {

                }
            }

            var siInterface = new SiInterface();

            int index = 5;

            if (args.Length > 1)
                index = int.Parse(args[1]);

            var device = SiInterface.GetAllDevices().ToList()[index];

            siInterface.SetCurrentDevice(device);
            
            switch (action)
            {
                case Action.ReadCards:
                    siInterface.SetCurrentTargetDevice(TargetDevice.Direct);

                    siInterface.SiCardRead += (sender, e) =>
                    {
                        Create(e.Cards[0]).Serialize($"latest.card");
                        Console.WriteLine($"Card {e.Cards[0].Siid} read!");
                    };

                    siInterface.ReadCards();

                    Console.WriteLine("Waiting for card insert...");
                    while (_continue) { }
                    break;
            }
        }

        static XMLDocument Create(SportidentCard card)
        {
            var node = new XMLNode("Card");

            node.AddAttribute("siid", card.Siid.ToString());

            var persData = new XMLNode("PersonalData");

            persData.AddAttribute("firstName", card.PersonalData.FirstName);
            persData.AddAttribute("lastName", card.PersonalData.LastName);
            persData.AddAttribute("sex", card.PersonalData.Sex);
            persData.AddAttribute("dateOfBirth", card.PersonalData.DateOfBirth);
            persData.AddAttribute("class", card.PersonalData.Class);
            persData.AddAttribute("club", card.PersonalData.Club);
            persData.AddAttribute("email", card.PersonalData.Email);
            persData.AddAttribute("phone", card.PersonalData.Phone);
            persData.AddAttribute("street", card.PersonalData.Street);
            persData.AddAttribute("city", card.PersonalData.City);
            persData.AddAttribute("country", card.PersonalData.Country);
            persData.AddAttribute("zipCode", card.PersonalData.ZipCode);

            var clearPunch = CreatePunchData(card.ClearPunch);
            clearPunch.Name = "ClearPunch";
            var clearPunchReserve = CreatePunchData(card.ClearPunchReserve);
            clearPunchReserve.Name = "ClearPunchReserve";
            var checkPunch = CreatePunchData(card.CheckPunch);
            checkPunch.Name = "CheckPunch";
            var startPunch = CreatePunchData(card.StartPunch);
            startPunch.Name = "StartPunch";
            var startPunchReserve = CreatePunchData(card.StartPunchReserve);
            startPunchReserve.Name = "StartPunchReserve";
            var finishPunch = CreatePunchData(card.FinishPunch);
            finishPunch.Name = "FinishPunch";
            var finishPunchReserve = CreatePunchData(card.FinishPunchReserve);
            finishPunchReserve.Name = "FinishPunchReserve";

            var controlPunches = new XMLNode("ControlPunches");

            foreach (var p in card.ControlPunchList)
            {
                var punch = CreatePunchData(p);
                punch.Name = "Punch";
                controlPunches.AddChild(punch);
            }

            node.AddChild(persData);
            node.AddChild(clearPunch);
            node.AddChild(clearPunchReserve);
            node.AddChild(checkPunch);
            node.AddChild(startPunch);
            node.AddChild(startPunchReserve);
            node.AddChild(finishPunch);
            node.AddChild(finishPunchReserve);
            node.AddChild(controlPunches);

            return new XMLDocument(node);

        }

        static XMLNode CreatePunchData(CardPunchData punch)
        {
            if (punch is null)
                return new XMLNode("Punch");


            var node = new XMLNode("Punch");

            node.AddAttribute("code", punch.CodeNumber.ToString());
            node.AddAttribute("siid", punch.Siid.ToString());

            node.AddAttribute("dayOfWeek", punch.PunchDateTime.DayOfWeek.ToString());
            node.AddAttribute("timeStamp", punch.PunchDateTime.Ticks.ToString());

            return node;
        }
    }
}
