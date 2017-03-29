using System;
using System.Threading.Tasks;
using Obvs.Extensions;
using Obvs.Types;

namespace Obvs
{
    public interface IServiceEndpoint : IServiceEndpoint<IMessage, ICommand, IEvent, IRequest, IResponse>
    {
    }

    public interface IServiceEndpoint<in TMessage, out TCommand, in TEvent, TRequest, in TResponse> : IEndpoint<TMessage>
        where TMessage : class
        where TCommand : TMessage
        where TEvent : TMessage
        where TRequest : TMessage
        where TResponse : TMessage
    {
        IObservable<TRequest> Requests { get; }
        IObservable<TCommand> Commands { get; }

        Task PublishAsync(TEvent ev);
        Task ReplyAsync(TRequest request, TResponse response);
    }

    public class ServiceEndpoint : ServiceEndpoint<IMessage, ICommand, IEvent, IRequest, IResponse>, IServiceEndpoint
    {
        public ServiceEndpoint(IMessageSource<IRequest> requestSource, IMessageSource<ICommand> commandSource, IMessagePublisher<IEvent> eventPublisher, IMessagePublisher<IResponse> responsePublisher, Type serviceType) 
            : base(requestSource, commandSource, eventPublisher, responsePublisher, serviceType)
        {
        }
    }

    public class ServiceEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse> : IServiceEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
        where TCommand : class, TMessage
        where TEvent : class, TMessage
        where TRequest : class, TMessage
        where TResponse : class, TMessage
    {
        private readonly IMessageSource<TRequest> _requestSource;
        private readonly IMessageSource<TCommand> _commandSource;
        private readonly IMessagePublisher<TEvent> _eventPublisher;
        private readonly IMessagePublisher<TResponse> _responsePublisher;
        private readonly string _name;

        public ServiceEndpoint(IMessageSource<TRequest> requestSource,
            IMessageSource<TCommand> commandSource,
            IMessagePublisher<TEvent> eventPublisher,
            IMessagePublisher<TResponse> responsePublisher,
            Type serviceType)
        {
            _requestSource = requestSource;
            _commandSource = commandSource;
            _eventPublisher = eventPublisher;
            _responsePublisher = responsePublisher;
            ServiceType = serviceType;
            _name = string.Format("{0}[{1}]", GetType().GetSimpleName(), ServiceType.Name);
        }

        public IObservable<TRequest> Requests
        {
            get { return _requestSource.Messages; }
        }

        public IObservable<TCommand> Commands
        {
            get { return _commandSource.Messages; }
        }

        public Task PublishAsync(TEvent ev)
        {
            return _eventPublisher.PublishAsync(ev);
        }

        public Task ReplyAsync(TRequest request, TResponse response)
        {
            return _responsePublisher.PublishAsync(response);
        }

        public bool CanHandle(TMessage message)
        {
            return ServiceType.GetInfo().IsInstanceOfType(message);
        }

        public string Name
        {
            get { return _name; }
        }

        private Type ServiceType { get; set; }

        public void Dispose()
        {
            _commandSource.Dispose();
            _eventPublisher.Dispose();
            _requestSource.Dispose();
            _responsePublisher.Dispose();
        }
    }
}