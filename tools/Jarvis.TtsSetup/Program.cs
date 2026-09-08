using Jarvis.Security;
using System.Drawing;
using System.Net.Http.Headers;
using System.Windows.Forms;

ApplicationConfiguration.Initialize();

var form = new Form
{
    Text = "Jarvis — konfiguracja ElevenLabs",
    StartPosition = FormStartPosition.CenterScreen,
    ClientSize = new Size(560, 245),
    FormBorderStyle = FormBorderStyle.FixedDialog,
    MaximizeBox = false,
    MinimizeBox = false,
    TopMost = true
};

var title = new Label
{
    Text = "Wklej klucz API ElevenLabs",
    AutoSize = true,
    Font = new Font("Segoe UI", 14, FontStyle.Bold),
    Location = new Point(24, 22)
};

var info = new Label
{
    Text = "Najpierw sprawdzę klucz w ElevenLabs. Poprawny klucz zapiszę tylko w Windows.",
    AutoSize = true,
    Location = new Point(27, 60)
};
var keyBox = new TextBox
{
    Location = new Point(28, 92),
    Size = new Size(500, 30),
    UseSystemPasswordChar = true,
    Font = new Font("Segoe UI", 11)
};

var showBox = new CheckBox
{
    Text = "Pokaż klucz",
    AutoSize = true,
    Location = new Point(28, 132)
};
showBox.CheckedChanged += (_, _) =>
    keyBox.UseSystemPasswordChar = !showBox.Checked;

var status = new Label
{
    Text = "",
    AutoSize = true,
    Location = new Point(28, 166)
};

var saveButton = new Button
{
    Text = "Sprawdź i zapisz",
    Size = new Size(145, 34),
    Location = new Point(383, 198)
};
var cancelButton = new Button
{
    Text = "Anuluj",
    Size = new Size(110, 34),
    Location = new Point(261, 198),
    DialogResult = DialogResult.Cancel
};

saveButton.Click += async (_, _) =>
{
    var secret = keyBox.Text.Trim();
    if (string.IsNullOrWhiteSpace(secret))
    {
        MessageBox.Show(form, "Wklej klucz API.", "Jarvis",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    saveButton.Enabled = false;
    status.Text = "Sprawdzam klucz...";
    try
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("xi-api-key", secret);
        http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        using var response = await http.GetAsync("https://api.elevenlabs.io/v2/voices?page_size=1");
        if (!response.IsSuccessStatusCode)
        {
            status.Text = "Klucz odrzucony przez ElevenLabs.";
            MessageBox.Show(form,
                $"ElevenLabs odrzucił klucz (HTTP {(int)response.StatusCode}). Sprawdź, czy skopiowałeś cały aktywny API key.",
                "Nieprawidłowy klucz", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var store = new WindowsCredentialSecretStore();
        store.Write("ElevenLabsApiKey", secret);
        keyBox.Clear();
        status.Text = "Klucz poprawny i zapisany.";
        MessageBox.Show(form,
            "Klucz jest poprawny i został zapisany w Windows Credential Manager.",
            "Jarvis", MessageBoxButtons.OK, MessageBoxIcon.Information);
        form.DialogResult = DialogResult.OK;
        form.Close();
    }
    catch (Exception ex)
    {
        status.Text = "Błąd połączenia lub zapisu.";
        MessageBox.Show(form, ex.Message, "Jarvis",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        saveButton.Enabled = true;
    }
};

form.Controls.AddRange([title, info, keyBox, showBox, status, cancelButton, saveButton]);
form.AcceptButton = saveButton;
form.CancelButton = cancelButton;
form.Shown += (_, _) => keyBox.Focus();
Application.Run(form);
