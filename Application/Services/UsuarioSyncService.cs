using System.Net.Http.Headers;
using System.Text.Json;
using c___Api_Example.data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using IbpvDtos; 

public class UsuarioSyncService
{
    private readonly IbpvDataBaseContext _context;
    private readonly IDataProtector _dataProtector;
    private readonly HttpClient _httpClient;

    public UsuarioSyncService(IbpvDataBaseContext context, IDataProtectionProvider dataProtectionProvider)
    {
        _context = context;
        _dataProtector = dataProtectionProvider.CreateProtector("userCryptography");

        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://192.168.1.201:5287/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9zaWQiOiI2MiIsImV4cCI6MTc0NjgzNjk5NX0.etqNUFS-SQmG2yFOaw9mV_vib3LTJOCFh54i1pEZcXU"); // Token completo aqui
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
    }

    public async Task SyncUsuariosAsync()
    {
        // Pega todos os usuários no banco de dados
        var usuarios = await _context.Usuarios.ToListAsync();
        if (usuarios == null || usuarios.Count == 0) return;

        foreach (var primeiroUsuario in usuarios)
        {
            // Realiza a requisição GET na API para pegar os dados do usuário
            var response = await _httpClient.GetAsync($"api/Usuario/{primeiroUsuario.Id}");
            if (!response.IsSuccessStatusCode) continue;

            // Processa a resposta da API
            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<UsuarioGetByIdDTO>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (dto == null) continue;

            // Recriptografa apenas os documentos (Cpf e RGnumero) utilizando DataProtector
            if (!string.IsNullOrEmpty(dto.Cpf))
            {
                dto.Cpf = _dataProtector.Protect(dto.Cpf);
            }

            if (!string.IsNullOrEmpty(dto.RGnumero))
            {
                dto.RGnumero = _dataProtector.Protect(dto.RGnumero);
            }

            // Atualiza apenas os dados necessários
            primeiroUsuario.Cpf = dto.Cpf; // Criptografado
            primeiroUsuario.RGnumero = dto.RGnumero; // Criptografado
            primeiroUsuario.RuaEdereco = dto.RuaEdereco; // Endereço sem criptografia
            primeiroUsuario.CepEndereco = dto.CepEndereco; // Endereço sem criptografia
            primeiroUsuario.NumeroEndereco = dto.NumeroEndereco; // Endereço sem criptografia
            primeiroUsuario.BairroEdereco = dto.BairroEdereco; // Endereço sem criptografia
            primeiroUsuario.CidadeEndereco = dto.CidadeEndereco; // Endereço sem criptografia
            primeiroUsuario.UfEndereco = dto.UfEndereco; // Endereço sem criptografia
            primeiroUsuario.ComplementoEndereco = dto.ComplementoEndereco; // Endereço sem criptografia

            // Salva as alterações no banco de dados
            await _context.SaveChangesAsync();
        }
    }
}
