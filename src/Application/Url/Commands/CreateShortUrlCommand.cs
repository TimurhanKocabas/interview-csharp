using FluentValidation;
using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Application.Common.Interfaces;

namespace UrlShortenerService.Application.Url.Commands;

public record CreateShortUrlCommand : IRequest<string>
{
    public string Url { get; init; } = default!;
    public string BaseReturnUrl { get; init; } = default!;
}

public class CreateShortUrlCommandValidator : AbstractValidator<CreateShortUrlCommand>
{
    public CreateShortUrlCommandValidator()
    {
        _ = RuleFor(v => v.Url)
          .NotEmpty()
          .WithMessage("Url is required.");
        _ = RuleFor(v => v.BaseReturnUrl)
            .NotEmpty()
            .WithMessage("BaseReturnUrl is required");
    }
}

public class CreateShortUrlCommandHandler : IRequestHandler<CreateShortUrlCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IHashids _hashids;

    public CreateShortUrlCommandHandler(IApplicationDbContext context, IHashids hashids)
    {
        _context = context;
        _hashids = hashids;
    }

    public async Task<string> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        if (!Uri.IsWellFormedUriString(request.Url, UriKind.Absolute))
        {
            throw new ArgumentException(request.Url + "is not a valid url");
        }
        if (string.IsNullOrWhiteSpace(request.BaseReturnUrl))
        {
            throw new ArgumentNullException("BaseReturnUrl is missing.");
        }

        var url = _context.Urls.FirstOrDefault(t => t.OriginalUrl == request.Url);
        if (url == null)
        {
            url = new Domain.Entities.Url()
            {
                OriginalUrl = request.Url
            };
            _context.Urls.Add(url);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return $"{request.BaseReturnUrl}/{_hashids.EncodeLong(url.Id)}";
    }
}
