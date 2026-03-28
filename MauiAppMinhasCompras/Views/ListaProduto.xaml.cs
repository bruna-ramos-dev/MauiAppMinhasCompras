using MauiAppMinhasCompras.Models;
using System.Collections.ObjectModel;

namespace MauiAppMinhasCompras.Views;

public partial class ListaProduto : ContentPage
{
    // ObservableCollection usada para atualizar a interface automaticamente
    ObservableCollection<Produto> lista = new ObservableCollection<Produto>();
    List<Produto> listaCompleta = new List<Produto>();
    public ListaProduto()
    {
        InitializeComponent();

        // Liga a lista à interface
        lst_produtos.ItemsSource = lista;
    }

    // Carrega os produtos ao abrir a tela
    protected async override void OnAppearing()
    {
        try
        {
            lista.Clear();

            listaCompleta = await App.Db.GetAll();

            listaCompleta.ForEach(i => lista.Add(i));

            // 🔽 CARREGAR CATEGORIAS NO PICKER
            var categorias = listaCompleta
                .Select(p => p.Categoria)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .ToList();

            picker_categoria.ItemsSource = categorias;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private void OnCategoriaSelecionada(object sender, EventArgs e)
    {
        try
        {
            if (picker_categoria.SelectedIndex == -1)
                return;

            string categoria = picker_categoria.SelectedItem.ToString();

            var filtrados = listaCompleta
                .Where(p => p.Categoria == categoria)
                .ToList();

            lista.Clear();
            filtrados.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            DisplayAlert("Ops", ex.Message, "OK");
        }
    }
    // Busca dinâmica com SearchBar
    private void txt_search_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            string q = e.NewTextValue?.ToLower() ?? "";

            var filtrados = listaCompleta
                .Where(p => p.Descricao.ToLower().Contains(q))
                .ToList();

            lista.Clear();
            filtrados.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            DisplayAlert("Ops", ex.Message, "OK");
        }
        finally
        {
            lst_produtos.IsRefreshing = false;
        }
    }


    // Soma total dos produtos
    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        double soma = lista.Sum(i => i.Total);

        string msg = $"O total é {soma:C}";

        DisplayAlert("Total dos Produtos", msg, "OK");
    }

    // Excluir produto
    private async void MenuItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            MenuItem selecinado = sender as MenuItem;

            Produto p = selecinado.BindingContext as Produto;

            bool confirm = await DisplayAlert(
                "Tem Certeza?", $"Remover {p.Descricao}?", "Sim", "Não");

            if (confirm)
            {
                await App.Db.Delete(p.Id);
                lista.Remove(p);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }
    private void lst_produtos_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            Produto p = e.SelectedItem as Produto;

            Navigation.PushAsync(new Views.EditarProduto
            {
                BindingContext = p,
            });
        }
        catch (Exception ex)
        {
            DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void lst_produtos_Refreshing(object sender, EventArgs e)
    {
        try
        {
            lista.Clear();

            listaCompleta = await App.Db.GetAll();

            listaCompleta.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
        finally
        {
            lst_produtos.IsRefreshing = false;
        }
    }

    private async void ToolbarItem_Relatorio(object sender, EventArgs e)
    {
        var relatorio = listaCompleta
            .GroupBy(p => p.Categoria)
            .Select(g => new
            {
                Categoria = g.Key,
                Total = g.Sum(p => p.Total)
            })
            .ToList();

        string msg = "";

        foreach (var item in relatorio)
        {
            msg += $"{item.Categoria}: {item.Total:C}\n";
        }

        await DisplayAlert("Gastos por Categoria", msg, "OK");
    }
    private void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            Navigation.PushAsync(new Views.NovoProduto());
        }
        catch (Exception ex)
        {
            DisplayAlert("Erro", ex.Message, "OK");
        }
    }
}