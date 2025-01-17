# Semantic Processes Framework Uruchomianie:

[Microsoft  dokumentacja](https://github.com/microsoft/semantic-kernel/blob/39934f5fa338141c8a64de96895a4e1f440638d7/dotnet/samples/GettingStartedWithProcesses/README.md)  
[LinkedIn przykład](https://www.linkedin.com/pulse/introducing-semantic-kernel-process-library-new-era-ai-latorre-g8tef)  
[Microsoft przykłady](https://github.com/microsoft/semantic-kernel/blob/39934f5fa338141c8a64de96895a4e1f440638d7/dotnet/samples/GettingStartedWithProcesses/Step00/Steps/DoSomeWorkStep.cs)


### Zdarzenie dla procesów - musza zostać zaimplementowane 

	public static class ChatBotEvents
	{
		public const string StartProcess = "StartProcess";
		public const string UserInputReceived = "UserInputReceived";
		public const string ResponseGenerated = "ResponseGenerated";
		public const string Exit = "Exit";
	}

## Automatyczne wywołanie nadpisanej funckji ActivateAsync
Zwróć uwagę, że wbudowane funkcje używają ValueTask do wywoływania zdarzeń. W naszych funkcjach używamy, przez nas zdefiniowanych, w tym przypadku - ChatBotEvents

	 [Experimental("SKEXP0080")]
	 public class TestIntroStep : KernelProcessStep<Message>
	 {
		 private Message? _state;
		 
		 public override ValueTask ActivateAsync(KernelProcessStepState<Message> message)
		 {
			 if (message.State == null)
			 {
				 message = message with { State = new Message() };
			 }
			 _state = message.State;
			 return ValueTask.CompletedTask;
		 }
	 }

### Wywolanie
Podczas jakiegokolwiek użycia obiektu przez proces, przed wykonaniem jakiegokolwiek innego procesu wywołana jest nadpisanafunckja ActivateAsync, która w tym przypadku inicjalizuje obiekt Message, który będzie używany pomiędzy procesami (stepami).


	process
		.OnInputEvent(ChatBotEvents.StartProcess)
		.SendEventTo(new ProcessFunctionTargetBuilder(introStep));



## Wywołanie KernelFunction PrintIntroMessage
	public class TestIntroStep : KernelProcessStep
	{
		[KernelFunction]
		public void PrintIntroMessage()  // ## 3
		{
			Debug.WriteLine("Welcome to the Advanced Semantic Kernel Chatbot");
		}
		
		...
	}

### Uruchamianie
W tym przypadku najpierw uruchomi się 'ActivateAsync' a następnie wywoła się PrintIntroMessage

	process
     .OnInputEvent(ChatBotEvents.StartProcess)
     .SendEventTo(new ProcessFunctionTargetBuilder(introStep, nameof(TestIntroStep.PrintIntroMessage)));


## Wywołanie po PrintIntroMessage SetMessageAsync w tej samej klasie
cała klassa TestIntroStep + kolejna do której przekażemy zapomocą parametru do kroku dane z TestIntroStep do 
ChatBotResponseStep.GetChatResponseAsync :

	public class TestIntroStep : KernelProcessStep<Message>
	{
		[KernelFunction]
		public void PrintIntroMessage()  // ## 3
		{
			Debug.WriteLine("Welcome to the Advanced Semantic Kernel Chatbot!\nType 'exit' at any time to quit.\n");
		}

		private Message? _state;

		public override ValueTask ActivateAsync(KernelProcessStepState<Message> message)
		{
			if (message.State == null)
			{
				message = message with { State = new Message() };
			}
			_state = message.State;
			return ValueTask.CompletedTask;
		}

		[KernelFunction("SetMessage")]
		public async ValueTask SetMessageAsync(KernelProcessStepContext context)  // ## 5 , 8
		{
			var input = "Wiadomość Hello World";
			_state!.Text = input;
			await context.EmitEventAsync(new() { Id = ChatBotEvents.UserInputReceived, Data = input });
		}
	}
//	
	
	[Experimental("SKEXP0080")]
	public class TestStepsOne : KernelProcessStep<Message2>  <-- to, że message2 to nie ma żadnego znaczenia, nic nie znaczy dlatego przykładu
	{
		private Message? _state;

		public override ValueTask ActivateAsync(KernelProcessStepState<Message2> message)
		{

			return ValueTask.CompletedTask;
		}

		[KernelFunction("MyReader")]
		public async ValueTask MyReaderAsync(KernelProcessStepContext context, string userMessage)
		{
			Debug.WriteLine($"Step - {userMessage}\n");
			await context.EmitEventAsync(new() { Id = ChatBotEvents.UserInputReceived, Data = _state.Text });
		}
	}
### Uruchamianie
Wywołanie sekwencji, po PrintIntroMessage z TestIntroStep wywoła się ActivateAsync w TestStepsOne.

Wyołanie ActivateAsync w TestIntroStep a zaraz po nim PrintIntroMessage
	process
     .OnInputEvent(ChatBotEvents.StartProcess)
     .SendEventTo(new ProcessFunctionTargetBuilder(introStep, nameof(TestIntroStep.PrintIntroMessage)));

Wywołanie kroku w momencie zakończenia funkcji PrintIntroMessage.
Tym krokiem będzie uruchomienie SetMessageAsync w TestIntroStep, która zapisze wartość w obiekcie Message klasy TestIntroStep
oraz input zostaje wstrzyknięty do KernelProcessEvent.Data. KernelProcessEvent jest emitowany pomiędzy krokami. 
Dodatkowo krok okrzykuje emituje event ChatBotEvents2.UserInputReceived do którego będziemy się podłączać.

	introStep
		.OnFunctionResult(nameof(TestIntroStep.PrintIntroMessage))
		.SendEventTo(new ProcessFunctionTargetBuilder(startStep));  <-- ActivateAsync po zakończeniu PrintIntroMessage

Teraz nasłuchuje ChatBotEvents2.UserInputReceived klasy TestIntroStep (introStep), jak MyReaderAsync emituje ChatBotEvents2.UserInputReceived to wywołam TestStepsOne z parameterName "userMessage". Automagicznie Procesy wiedzą, żejest funkcja
z takim argumentem i automatycznie wywoła tą funkcję wstrykując przez ten parametr KernelProcessEvent.Data, która została uzupełniona
w SetMessageAsync

	introStep
		.OnEvent(ChatBotEvents2.UserInputReceived)  <-- on event bo ChatBotEvents.UserInputReceived
		.SendEventTo(new ProcessFunctionTargetBuilder(startStep, parameterName: "userMessage"));


## Całość

	[Experimental("SKEXP0080")]
	public class TestIntroStep : KernelProcessStep<Message>
	{
		[KernelFunction]
		public void PrintIntroMessage()  // ## 3
		{
			Debug.WriteLine("Welcome to the Advanced Semantic Kernel Chatbot!\nType 'exit' at any time to quit.\n");
		}

		private Message? _state;

		public override ValueTask ActivateAsync(KernelProcessStepState<Message> message)
		{
			if (message.State == null)
			{
				message = message with { State = new Message() };
			}
			_state = message.State;
			return ValueTask.CompletedTask;
		}

		[KernelFunction("SetMessage")]
		public async ValueTask SetMessageAsync(KernelProcessStepContext context)  // ## 5 , 8
		{
			var input = "Wiadomość Hello World";
			_state!.Text = input;
			await context.EmitEventAsync(new() { Id = ChatBotEvents.UserInputReceived, Data = input });
		}
	}

	[Experimental("SKEXP0080")]
	public class TestStepsOne : KernelProcessStep<Message2>
	{
		private Message? _state;

		public override ValueTask ActivateAsync(KernelProcessStepState<Message2> message)
		{

			return ValueTask.CompletedTask;
		}

		[KernelFunction("MyReader")]
		public async ValueTask MyReaderAsync(KernelProcessStepContext context, string userMessage)
		{
			Debug.WriteLine($"Step - {userMessage}\n");
			await context.EmitEventAsync(new() { Id = ChatBotEvents.UserInputReceived, Data = _state.Text });  <-- wstrzykujemy dane do KernelProcessEvent.Data, który jest wstrzykiwany pomiędzy krokami. Dodatkowo emitujemy sygnał UserInputReceived
		}
	}
//

	ProcessBuilder process = new("ChatBot");
	var introStep = process.AddStepFromType<TestIntroStep>();
	Kernel kernel = new AgentCollection(_configuration).AddOpenAIChatCompletion();

	const string SetMessage = nameof(SetMessage);

	process
	 .OnInputEvent(ChatBotEvents.StartProcess)
	 .SendEventTo(new ProcessFunctionTargetBuilder(introStep, nameof(TestIntroStep.PrintIntroMessage)));

	introStep
	 .OnFunctionResult(nameof(TestIntroStep.PrintIntroMessage))
	 .SendEventTo(new ProcessFunctionTargetBuilder(introStep, nameof(SetMessage)));


	introStep
	 .OnEvent(ChatBotEvents2.UserInputReceived)  <-- onevent bo ChatBotEvents.UserInputReceived
	 .SendEventTo(new ProcessFunctionTargetBuilder(startStep, parameterName: "userMessage"));

	KernelProcess kernelProcess = process.Build();

	using var runningProcess = await kernelProcess.StartAsync(
	 kernel,
	new KernelProcessEvent()
	{
		Id = ProcessEvents.StartProcess,  <-- to jest punkt startowy, w pierwszym kroku aby uruchomić proces przekazujemy EventId = ProcessEvents.StartProcess (.OnInputEvent(ChatBotEvents.StartProcess))
		Data = null
	});

