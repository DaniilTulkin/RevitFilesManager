using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RevitFilesManager
{
    [ValueConversion(typeof(DirectoryItemType), typeof(BitmapImage))]
    public class HeaderToImageConverter : IValueConverter
    {
        public static HeaderToImageConverter Instance = new HeaderToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string image = string.Empty;
            switch ((DirectoryItemType)value)
            {
                case DirectoryItemType.RootType:
                    image = "root";
                    break;
                case DirectoryItemType.FolderType:
                    image = "folder";
                    break;
                case DirectoryItemType.FileType:
                    image = "file";
                    break;
            }
            return new BitmapImage(new Uri($"pack://application:,,,/RevitFilesManager;component/Resources/Icons/{image}.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
