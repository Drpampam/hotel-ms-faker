using hotelier_core_app.API.Controllers;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.States;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RoomControllerTests
{
    private readonly IRoomService _roomService = Substitute.For<IRoomService>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IHttpContextAccessor _accessor = Substitute.For<IHttpContextAccessor>();
    private readonly RoomController _controller;

    public RoomControllerTests()
    {
        _controller = new RoomController(_roomService, _tokenService, _accessor);
    }

    [Fact]
    public async Task ChangeRoomState_ReturnsOk_WhenSuccess()
    {
        _roomService.ChangeRoomStateAsync(Arg.Any<long>(), Arg.Any<RoomTrigger>())
            .Returns(Task.FromResult(new BaseResponse<RoomStateResponseDTO>
            {
                Status = true,
                Data = new RoomStateResponseDTO()
            }));
        var result = await _controller.ChangeRoomState(1, RoomTrigger.SetCleaning);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ChangeRoomState_ReturnsBadRequest_WhenFailed()
    {
        _roomService.ChangeRoomStateAsync(Arg.Any<long>(), Arg.Any<RoomTrigger>())
            .Returns(Task.FromResult(new BaseResponse<RoomStateResponseDTO>
            {
                Status = false,
                Data = null
            }));
        var result = await _controller.ChangeRoomState(1, RoomTrigger.SetCleaning);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetRoomState_ReturnsOk_WhenStateExists()
    {
        _roomService.GetRoomStateAsync(Arg.Any<long>())
            .Returns(Task.FromResult(new BaseResponse<RoomStateResponseDTO>
            {
                Status = true,
                Data = new RoomStateResponseDTO()
            }));
        var result = await _controller.GetRoomState(1);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetRoomState_ReturnsNotFound_WhenStateIsNull()
    {
        _roomService.GetRoomStateAsync(Arg.Any<long>())
            .Returns(Task.FromResult(BaseResponse<RoomStateResponseDTO>.Failure(
                new RoomStateResponseDTO(),
                "Room not found",
                ResponseStatusCode.NoRecordFound)));

        var result = await _controller.GetRoomState(1);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetRoomState_ReturnsBadRequest_WhenFailureIsNotNotFound()
    {
        _roomService.GetRoomStateAsync(Arg.Any<long>())
            .Returns(Task.FromResult(BaseResponse<RoomStateResponseDTO>.Failure(
                new RoomStateResponseDTO(),
                "Invalid room state",
                ResponseStatusCode.InvalidData)));

        var result = await _controller.GetRoomState(1);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetAvailableRoomTriggers_ReturnsOk_WhenTriggersExist()
    {
        _roomService.GetAvailableTriggersAsync(Arg.Any<long>())
            .Returns(Task.FromResult(new BaseResponse<List<RoomTrigger>>
            {
                Status = true,
                Data = new List<RoomTrigger> { RoomTrigger.SetCleaning }
            }));
        var result = await _controller.GetAvailableRoomTriggers(1);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAvailableRoomTriggers_ReturnsNotFound_WhenTriggersNull()
    {
        var result = await _controller.GetAvailableRoomTriggers(1);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ChangeRoomState_ReturnsBadRequest_WhenIdIsInvalid()
    {
        _roomService.ChangeRoomStateAsync(-1, Arg.Any<RoomTrigger>())
            .Returns(Task.FromResult(new BaseResponse<RoomStateResponseDTO>
            {
                Status = false,
                Data = null
            }));
        var result = await _controller.ChangeRoomState(-1, RoomTrigger.SetCleaning);
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
