using System;
using NUnit.Util;
using NUnit.Core;
using NUnit.Framework;

namespace NUnit.Tests
{
	/// <summary>
	/// Summary description for EventDispatcherTests.
	/// </summary>
	[TestFixture]
	public class EventDispatcherTests
	{
		private TestEventDispatcher dispatcher;
		private TestEventCatcher catcher;
		private Test test;
		private TestResult result;
		private Exception exception;

		private readonly string FILENAME = "MyTestFileName";
		private readonly string TESTNAME = "MyTestName";
		private readonly string MESSAGE = "My message!";
		private readonly string RSLTNAME = "MyResult";

		[SetUp]
		public void SetUp()
		{
			dispatcher = new TestEventDispatcher();
			catcher = new TestEventCatcher( dispatcher );
			test = new TestSuite( TESTNAME );
			result = new TestSuiteResult( test, RSLTNAME );
			exception = new Exception( MESSAGE );
		}

		[Test]
		public void ProjectLoading()
		{
			dispatcher.FireProjectLoading( FILENAME );
			CheckEvent( TestAction.ProjectLoading, FILENAME );
		}

		[Test]
		public void ProjectLoaded()
		{
			dispatcher.FireProjectLoaded( FILENAME );
			CheckEvent( TestAction.ProjectLoaded, FILENAME );
		}

		[Test]
		public void ProjectLoadFailed()
		{
			dispatcher.FireProjectLoadFailed( FILENAME, exception );
			CheckEvent( TestAction.ProjectLoadFailed, FILENAME, exception );
		}

		[Test]
		public void ProjectUnloading()
		{
			dispatcher.FireProjectUnloading( FILENAME );
			CheckEvent( TestAction.ProjectUnloading, FILENAME );
		}

		[Test]
		public void ProjectUnloaded()
		{
			dispatcher.FireProjectUnloaded( FILENAME );
			CheckEvent( TestAction.ProjectUnloaded, FILENAME );
		}

		[Test]
		public void ProjectUnloadFailed()
		{
			dispatcher.FireProjectUnloadFailed( FILENAME, exception );
			CheckEvent( TestAction.ProjectUnloadFailed, FILENAME, exception );
		}

		[Test]
		public void TestLoading()
		{
			dispatcher.FireTestLoading( FILENAME );
			CheckEvent( TestAction.TestLoading, FILENAME );
		}

		[Test]
		public void TestLoaded()
		{
			dispatcher.FireTestLoaded( FILENAME, test );
			CheckEvent( TestAction.TestLoaded, FILENAME, test );
		}

		[Test]
		public void TestLoadFailed()
		{
			dispatcher.FireTestLoadFailed( FILENAME, exception );
			CheckEvent( TestAction.TestLoadFailed, FILENAME, exception );
		}

		[Test]
		public void TestUnloading()
		{
			dispatcher.FireTestUnloading( FILENAME, test );
			CheckEvent( TestAction.TestUnloading, FILENAME );
		}

		[Test]
		public void TestUnloaded()
		{
			dispatcher.FireTestUnloaded( FILENAME, test );
			CheckEvent( TestAction.TestUnloaded, FILENAME, test );
		}

		[Test]
		public void TestUnloadFailed()
		{
			dispatcher.FireTestUnloadFailed( FILENAME, exception );
			CheckEvent( TestAction.TestUnloadFailed, FILENAME, exception );
		}

		[Test]
		public void TestReloading()
		{
			dispatcher.FireTestReloading( FILENAME, test );
			CheckEvent( TestAction.TestReloading, FILENAME );
		}

		[Test]
		public void TestReloaded()
		{
			dispatcher.FireTestReloaded( FILENAME, test );
			CheckEvent( TestAction.TestReloaded, FILENAME, test );
		}

		[Test]
		public void TestReloadFailed()
		{
			dispatcher.FireTestReloadFailed( FILENAME, exception );
			CheckEvent( TestAction.TestReloadFailed, FILENAME, exception );
		}

		[Test]
		public void RunStarting()
		{
			dispatcher.FireRunStarting( test );
			CheckEvent( TestAction.RunStarting, test );
		}

		[Test]
		public void RunFinished()
		{
			dispatcher.FireRunFinished( result );
			CheckEvent( TestAction.RunFinished, result );
		}

		[Test]
		public void RunFailed()
		{
			dispatcher.FireRunFinished( exception );
			CheckEvent( TestAction.RunFinished, exception );
		}

		[Test]
		public void SuiteStarting()
		{
			dispatcher.FireSuiteStarting( test );
			CheckEvent( TestAction.SuiteStarting, test );
		}

		[Test]
		public void SuiteFinished()
		{
			dispatcher.FireSuiteFinished( result );
			CheckEvent( TestAction.SuiteFinished, result );
		}

		[Test]
		public void TestStarting()
		{
			dispatcher.FireTestStarting( test );
			CheckEvent( TestAction.TestStarting, test );
		}

		[Test]
		public void TestFinished()
		{
			dispatcher.FireTestFinished( result );
			CheckEvent( TestAction.TestFinished, result );
		}

		private void CheckEvent( TestAction action )
		{
			Assert.Equals( 1, catcher.Events.Count );
			Assert.Equals( action, catcher.Events[0].Action );
		}

		private void CheckEvent( TestAction action, string fileName )
		{
			CheckEvent( action );
			Assert.Equals( fileName, catcher.Events[0].TestFileName );
		}

		private void CheckEvent( TestAction action, string fileName, Test test )
		{
			CheckEvent( action, fileName );
			Assert.Equals( TESTNAME, catcher.Events[0].Test.Name );
		}

		private void CheckEvent( TestAction action, string fileName, Exception exception )
		{
			CheckEvent( action, fileName );
			Assert.Equals( MESSAGE, catcher.Events[0].Exception.Message );
		}

		private void CheckEvent( TestAction action, Test test )
		{
			CheckEvent( action );
			Assert.Equals( TESTNAME, catcher.Events[0].Test.Name );
		}

		private void CheckEvent( TestAction action, TestResult result )
		{
			CheckEvent( action );
			Assert.Equals( RSLTNAME, result.Name );
		}

		private void CheckEvent( TestAction action, Exception exception )
		{
			CheckEvent( TestAction.RunFinished );
			Assert.Equals( MESSAGE, catcher.Events[0].Exception.Message );
		}
	}
}
