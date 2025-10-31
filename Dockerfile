# syntax=docker/dockerfile:1

# Đặt phiên bản .NET (8.0)
ARG DOTNET_VERSION=8.0

# --- Tầng Build ---
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS builder
WORKDIR /src

# Sao chép các file .sln và .csproj
COPY SocialNetworkMobile.sln .
COPY SocialNetworkMobile/SocialNetworkMobile.csproj SocialNetworkMobile/
COPY SocialNetworkMobile.Repository/SocialNetworkMobile.Repository.csproj SocialNetworkMobile.Repository/
COPY SocialNetworkMobile.Services/SocialNetworkMobile.Services.csproj SocialNetworkMobile.Services/

# Restore các package, sử dụng cache
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.cache/msbuild \
    dotnet restore "SocialNetworkMobile/SocialNetworkMobile.csproj"

# Sao chép toàn bộ source code còn lại
COPY . .

# Build và publish project
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.cache \
    dotnet publish "SocialNetworkMobile/SocialNetworkMobile.csproj" -c Release -o /app/publish --no-restore

# --- Tầng Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app

# Bảo mật: Tạo user không phải root
RUN addgroup --system nexora && adduser --system --ingroup nexora nexorauser
USER nexorauser

# Sao chép kết quả build từ tầng 'builder'
COPY --from=builder /app/publish .

# Mở cổng 5078 (cổng nội bộ bên trong container)
EXPOSE 5078
ENV ASPNETCORE_URLS=http://+:5078

# Entrypoint (đã cập nhật .dll mới)
ENTRYPOINT ["dotnet", "SocialNetworkMobile.dll"]

