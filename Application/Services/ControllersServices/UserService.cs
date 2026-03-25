using AutoMapper;
using c___Api_Example.Application.Services.Gdrive;
using c___Api_Example.Application.Services.UserCryptography;
using c___Api_Example.Models;
using c___Api_Example.repository.Interfaces;
using IbpvDtos;
using Microsoft.AspNetCore.DataProtection;

namespace c___Api_Example.Application.Services.ControllersServices;

public class UserService
{
    private readonly IUsuarioRepositorio _usuarioRepositorio;
    private readonly IServiceUserPassCryptography _serviceUserPassCryptography;
    private readonly IDataProtector _dataProtector;
    private readonly IMapper _mapper;
    private readonly MinioService _minioService;

    public UserService(
        IUsuarioRepositorio usuarioRepositorio,
        IServiceUserPassCryptography serviceUserPassCryptography,
        IDataProtectionProvider dataProtectionProvider,
        IMapper mapper,
        MinioService minioService)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _serviceUserPassCryptography = serviceUserPassCryptography;
        _dataProtector = dataProtectionProvider.CreateProtector("userCryptography");
        _mapper = mapper;
        _minioService = minioService;
    }

    public async Task<PaginetedResultDTO<UsuarioPagDTO>> GetPaginated(
        int page, int itensQuantity, string? nome, string? token)
    {
        return await _usuarioRepositorio.GetPagUsers(page, itensQuantity, nome, token);
    }

    public async Task<UsuarioGetByIdDTO?> GetById(int id)
    {
        var user = await _usuarioRepositorio.GetUserById(id);
        if (user is null)
            return null;

        var dto = _mapper.Map<UsuarioGetByIdDTO>(user);

        if (!string.IsNullOrEmpty(dto.Cpf))
            dto.Cpf = _dataProtector.Unprotect(dto.Cpf);

        if (!string.IsNullOrEmpty(dto.RGnumero))
            dto.RGnumero = _dataProtector.Unprotect(dto.RGnumero);

        if (!string.IsNullOrEmpty(dto.urlImage))
        {
            dto.urlImage = await _minioService.GetPresignedUrlAsync(
                bucketName: "users",
                objectName: dto.urlImage,
                expiresInSeconds: 600
            );
        }
        
        return dto;
    }

    public async Task<int> Create(UsuarioPostDTO dto)
    {

        var existUserEmail = await _usuarioRepositorio.GetUserByGmail(dto.Email);
        if(existUserEmail is not null)
            throw new Exception("Email already exists");
        
        dto.Data_nascimento = DateTime.SpecifyKind(dto.Data_nascimento!.Value, DateTimeKind.Utc);

        if (dto.dataBatismo.HasValue)
            dto.dataBatismo = DateTime.SpecifyKind(dto.dataBatismo.Value, DateTimeKind.Utc);

        if (!string.IsNullOrEmpty(dto.Cpf))
            dto.Cpf = _dataProtector.Protect(dto.Cpf);

        if (!string.IsNullOrEmpty(dto.RGnumero))
            dto.RGnumero = _dataProtector.Protect(dto.RGnumero);
        
        if (string.IsNullOrWhiteSpace(dto.Senha))
        {
            dto.Senha = _serviceUserPassCryptography.GenerateTemporaryPassword();
        }

        dto.Senha = _serviceUserPassCryptography.encryptPassUser(dto.Senha!);

        var image = dto.Image;
        var model = _mapper.Map<UsuarioModel>(dto);
        
        if (image is not null && image.Length > 0)
        {
            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            ms.Position = 0;

            var objectName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

            await _minioService.UploadAsync(
                bucket: "users",
                stream: ms,
                objectName: objectName
            );

            model.urlImage = objectName;
        }
        
        
        return await _usuarioRepositorio.AddUser(model);
    }

    public async Task<bool> Update(int id, UsuarioPutDTO dto)
    {
        var dbUser= await _usuarioRepositorio.GetUserById(id);

        if (dbUser is null)
            throw new Exception("Usuário não encontrado");

        var emailExists = await _usuarioRepositorio
            .EmailExistsForAnotherUser(id, dto.Email);

        if (emailExists)
            throw new Exception("Email already exists");
       

      
        dto.Data_nascimento =
            DateTime.SpecifyKind(dto.Data_nascimento!.Value, DateTimeKind.Utc);

        if (dto.dataBatismo.HasValue)
            dto.dataBatismo =
                DateTime.SpecifyKind(dto.dataBatismo.Value, DateTimeKind.Utc);

    
        if (!string.IsNullOrEmpty(dto.Cpf))
            dbUser.Cpf = _dataProtector.Protect(dto.Cpf);

        if (!string.IsNullOrEmpty(dto.RGnumero))
            dbUser.RGnumero = _dataProtector.Protect(dto.RGnumero);

 
        if (dto.Image is not null && dto.Image.Length > 0)
        {
          
            if (!string.IsNullOrEmpty(dbUser.urlImage))
            {
                await _minioService.DeleteAsync(dbUser.urlImage, "users");
            }

            using var ms = new MemoryStream();
            await dto.Image.CopyToAsync(ms);
            ms.Position = 0;

            var objectName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";

            await _minioService.UploadAsync(
                bucket: "users",
                stream: ms,
                objectName: objectName
            );

            dbUser.urlImage = objectName;
        }

        
        dbUser.BairroEdereco = dto.BairroEdereco;
        dbUser.CidadeEndereco = dto.CidadeEndereco;
        dbUser.RuaEdereco = dto.RuaEdereco;
        dbUser.CepEndereco = dto.CepEndereco;
        dbUser.NumeroEndereco = dto.NumeroEndereco;
        dbUser.UfEndereco = dto.UfEndereco;
        
        if (!string.IsNullOrEmpty(dto.ComplementoEndereco))
            dbUser.ComplementoEndereco = dto.ComplementoEndereco;
        
        if (!string.IsNullOrEmpty(dto.pastorBatismo))
            dbUser.pastorBatismo = dto.pastorBatismo;

        if (!string.IsNullOrEmpty(dto.igrejaBatismo))
            dbUser.igrejaBatismo = dto.igrejaBatismo;
        
        dbUser.filhos = dto.filhos;
        
        if (!string.IsNullOrEmpty(dto.profissao))
            dbUser.profissao = dto.profissao;
        
 
        
        dbUser.Rule = dto.Rule;
        dbUser.genero = dto.genero;
        dbUser.alarmAuth = dto.alarmAuth;
        dbUser.Nome = dto.Nome;
        dbUser.Email = dto.Email;
        dbUser.Data_nascimento = dto.Data_nascimento.Value;
        dbUser.dataBatismo = dto.dataBatismo;
        dbUser.genero = dto.genero;
        dbUser.estadoCivil = dto.estadoCivil;
        
        dbUser.TelefoneNumero = dto.TelefoneNumero;
        dbUser.Telefone_pais = dto.Telefone_pais;
        
        return await _usuarioRepositorio.UpdateUser(dbUser);
    }

    public async Task<bool> Delete(int id)
    {
        var user = await _usuarioRepositorio.GetUserById(id);

        if (user is null)
            throw new Exception("Usuário não encontrado");
        
        if (!string.IsNullOrEmpty(user.urlImage))
        {
            await _minioService.DeleteAsync(user.urlImage, "users");
        }
        return await _usuarioRepositorio.DeleteUserById(id);
    }
}