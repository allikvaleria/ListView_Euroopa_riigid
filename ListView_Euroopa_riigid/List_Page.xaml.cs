using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ListView_Euroopa_riigid;

public partial class List_Page : ContentPage
{
    // 📌 Модель страны (внутри страницы)
    public class Riik
    {
        public string Nimi { get; set; }       // Название страны
        public string Pealinn { get; set; }    // Столица
        public int Elanikud { get; set; }      // Население
        public string Lipp { get; set; }       // Путь к флагу
    }

    // 🔁 Список стран
    private ObservableCollection<Riik> Riigid { get; set; }

    // UI элементы
    ListView listView;
    Entry nimiEntry, pealinnEntry, elanikudEntry;
    Button lisaBtn, kustutaBtn, valiLippBtn;
    Image lippImage;
    string valitudLipp;

    public List_Page()
    {
        Title = "Euroopa riigid";

        // 🏳️ Примерные страны
        Riigid = new ObservableCollection<Riik>
        {
            new Riik { Nimi = "Eesti", Pealinn = "Tallinn", Elanikud = 1325000, Lipp = "eesti.png" },
            new Riik { Nimi = "Soome", Pealinn = "Helsinki", Elanikud = 5536000, Lipp = "soome.png" }
        };

        // 🔤 Поля ввода
        nimiEntry = new Entry { Placeholder = "Riigi nimi" };
        pealinnEntry = new Entry { Placeholder = "Pealinn" };
        elanikudEntry = new Entry { Placeholder = "Elanike arv", Keyboard = Keyboard.Numeric };

        // 📸 Выбор фото
        valiLippBtn = new Button { Text = "Vali lipp" };
        valiLippBtn.Clicked += ValiLipp_Clicked;

        lippImage = new Image { WidthRequest = 80, HeightRequest = 50, Source = "placeholder.png" };

        // ➕ ➖ Кнопки
        lisaBtn = new Button { Text = "Lisa riik" };
        lisaBtn.Clicked += Lisa_Clicked;

        kustutaBtn = new Button { Text = "Kustuta riik" };
        kustutaBtn.Clicked += Kustuta_Clicked;

        // 📃 Список стран
        listView = new ListView
        {
            ItemsSource = Riigid,
            ItemTemplate = new DataTemplate(() =>
            {
                var lipp = new Image { WidthRequest = 50, HeightRequest = 30 };
                lipp.SetBinding(Image.SourceProperty, "Lipp");

                var nimi = new Label { FontAttributes = FontAttributes.Bold };
                nimi.SetBinding(Label.TextProperty, "Nimi");

                var pealinn = new Label();
                pealinn.SetBinding(Label.TextProperty, "Pealinn");

                return new ViewCell
                {
                    View = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Padding = new Thickness(10, 5),
                        Children =
                        {
                            lipp,
                            new StackLayout
                            {
                                Children = { nimi, pealinn },
                                VerticalOptions = LayoutOptions.Center
                            }
                        }
                    }
                };
            })
        };
        listView.ItemTapped += ListView_ItemTapped;

        // 📐 Разметка
        Content = new ScrollView
        {
            Content = new StackLayout
            {
                Padding = 10,
                Spacing = 10,
                Children =
                {
                    new Label { Text = "Sisesta uus riik:", FontAttributes = FontAttributes.Bold, FontSize = 18 },
                    nimiEntry,
                    pealinnEntry,
                    elanikudEntry,
                    valiLippBtn,
                    lippImage,
                    lisaBtn,
                    kustutaBtn,
                    new Label { Text = "Riikide loetelu:", FontSize = 16, FontAttributes = FontAttributes.Bold },
                    listView
                }
            }
        };
    }

    // ✅ Добавление страны
    private void Lisa_Clicked(object sender, EventArgs e)
    {
        string nimi = nimiEntry.Text?.Trim();
        string pealinn = pealinnEntry.Text?.Trim();
        string elanikudText = elanikudEntry.Text?.Trim();

        if (string.IsNullOrEmpty(nimi) || string.IsNullOrEmpty(pealinn) || string.IsNullOrEmpty(elanikudText)
            || !int.TryParse(elanikudText, out int elanikud))
        {
            DisplayAlert("Viga", "Täida kõik väljad õigesti!", "OK");
            return;
        }

        if (Riigid.Any(r => r.Nimi.Equals(nimi, System.StringComparison.OrdinalIgnoreCase)))
        {
            DisplayAlert("Hoiatus", "See riik on juba olemas!", "OK");
            return;
        }

        Riigid.Add(new Riik
        {
            Nimi = nimi,
            Pealinn = pealinn,
            Elanikud = elanikud,
            Lipp = string.IsNullOrEmpty(valitudLipp) ? "placeholder.png" : valitudLipp
        });

        // ПОчистка полей
        nimiEntry.Text = pealinnEntry.Text = elanikudEntry.Text = string.Empty;
        valitudLipp = null;
        lippImage.Source = "placeholder.png";
    }

    // ❌ Удаление страны
    private void Kustuta_Clicked(object sender, EventArgs e)
    {
        if (listView.SelectedItem is Riik valitud)
        {
            Riigid.Remove(valitud);
            listView.SelectedItem = null;
        }
    }

    // 📷 Загрузка флага
    private async void ValiLipp_Clicked(object sender, EventArgs e)
    {
        FileResult foto = await MediaPicker.Default.PickPhotoAsync();
        if (foto != null)
        {
            string tee = Path.Combine(FileSystem.CacheDirectory, foto.FileName);
            using var voog = await foto.OpenReadAsync();
            using var fail = File.OpenWrite(tee);
            await voog.CopyToAsync(fail);

            valitudLipp = tee;
            lippImage.Source = ImageSource.FromFile(tee);
        }
    }

    // 🔍 Информация о стране
    private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is Riik r)
        {
            await DisplayAlert("Riigi info",
                $"Nimi: {r.Nimi}\nPealinn: {r.Pealinn}\nElanikud: {r.Elanikud:N0}",
                "OK");
        }
    }
}
