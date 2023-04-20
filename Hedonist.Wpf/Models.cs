using Hedonist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hedonist.Wpf {

    //public enum GiftType {
    //    Art = 1,
    //    Music = 2,
    //    Trends = 3,
    //    FoodYa = 41,
    //    FoodStore = 42,
    //    Movement = 5,
    //    Mixology = 6,
    //    Communication = 7
    //}

    public class GiftPageModel {
        public string Ticket { get; set; }
        public List<Answer> SelectedAnswers { get; set; } = new();
    }
    public class GiftTypeRelation {
        public int GiftType { get; set; }
        public string BgImageName { get; set; }
    }

    public class GiftQrCodeCompleteData {
        public GiftQrCodeRawData RawData { get; set; }
        public BitmapImage QrCodeImageSource { get; set; }
    }
}
