using Shared.Wrappers;
using MediatR;
using Shared.RequestModels.Identity.Tokens;

namespace Application.Features.Identity.Tokens.Queries
{
    public class LoginQuery : IRequest<IResponseWrapper>
    {
        public TokenRequest TokenRequest { get; set; }
    }

    public class LoginQueryHandler(ITokenService tokenService) : IRequestHandler<LoginQuery, IResponseWrapper>
    {
        private readonly ITokenService _tokenService = tokenService;

        public async Task<IResponseWrapper> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var token = await _tokenService.LoginAsync(request.TokenRequest);

            return await ResponseWrapper<TokenResponse>.SuccessAsync(data: token);
        }
    }
}
