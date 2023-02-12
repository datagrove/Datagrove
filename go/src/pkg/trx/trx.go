package trx

import "encoding/xml"

type UnitTestResult struct {
	Text                     string `xml:",chardata"`
	ExecutionId              string `xml:"executionId,attr"`
	TestId                   string `xml:"testId,attr"`
	TestName                 string `xml:"testName,attr"`
	ComputerName             string `xml:"computerName,attr"`
	Duration                 string `xml:"duration,attr"`
	StartTime                string `xml:"startTime,attr"`
	EndTime                  string `xml:"endTime,attr"`
	TestType                 string `xml:"testType,attr"`
	Outcome                  string `xml:"outcome,attr"`
	TestListId               string `xml:"testListId,attr"`
	RelativeResultsDirectory string `xml:"relativeResultsDirectory,attr"`
	Output                   struct {
		Text      string `xml:",chardata"`
		StdOut    string `xml:"StdOut"`
		ErrorInfo struct {
			Text       string `xml:",chardata"`
			Message    string `xml:"Message"`
			StackTrace string `xml:"StackTrace"`
		} `xml:"ErrorInfo"`
	} `xml:"Output"`
}

// XML schema of the TRX file
type TestRun struct {
	XMLName xml.Name `xml:"TestRun"`
	Text    string   `xml:",chardata"`
	ID      string   `xml:"id,attr"`
	Name    string   `xml:"name,attr"`
	Xmlns   string   `xml:"xmlns,attr"`
	Times   struct {
		Text     string `xml:",chardata"`
		Creation string `xml:"creation,attr"`
		Queuing  string `xml:"queuing,attr"`
		Start    string `xml:"start,attr"`
		Finish   string `xml:"finish,attr"`
	} `xml:"Times"`
	TestSettings struct {
		Text       string `xml:",chardata"`
		Name       string `xml:"name,attr"`
		ID         string `xml:"id,attr"`
		Deployment struct {
			Text              string `xml:",chardata"`
			RunDeploymentRoot string `xml:"runDeploymentRoot,attr"`
		} `xml:"Deployment"`
	} `xml:"TestSettings"`
	Results struct {
		Text           string           `xml:",chardata"`
		UnitTestResult []UnitTestResult `xml:"UnitTestResult"`
	} `xml:"Results"`
	TestDefinitions struct {
		Text     string `xml:",chardata"`
		UnitTest []struct {
			Text      string `xml:",chardata"`
			Name      string `xml:"name,attr"`
			Storage   string `xml:"storage,attr"`
			ID        string `xml:"id,attr"`
			Execution struct {
				Text string `xml:",chardata"`
				ID   string `xml:"id,attr"`
			} `xml:"Execution"`
			TestMethod struct {
				Text            string `xml:",chardata"`
				CodeBase        string `xml:"codeBase,attr"`
				AdapterTypeName string `xml:"adapterTypeName,attr"`
				ClassName       string `xml:"className,attr"`
				Name            string `xml:"name,attr"`
			} `xml:"TestMethod"`
			TestCategory struct {
				Text             string `xml:",chardata"`
				TestCategoryItem struct {
					Text         string `xml:",chardata"`
					TestCategory string `xml:"TestCategory,attr"`
				} `xml:"TestCategoryItem"`
			} `xml:"TestCategory"`
		} `xml:"UnitTest"`
	} `xml:"TestDefinitions"`
	TestEntries struct {
		Text      string `xml:",chardata"`
		TestEntry []struct {
			Text        string `xml:",chardata"`
			TestId      string `xml:"testId,attr"`
			ExecutionId string `xml:"executionId,attr"`
			TestListId  string `xml:"testListId,attr"`
		} `xml:"TestEntry"`
	} `xml:"TestEntries"`
	TestLists struct {
		Text     string `xml:",chardata"`
		TestList []struct {
			Text string `xml:",chardata"`
			Name string `xml:"name,attr"`
			ID   string `xml:"id,attr"`
		} `xml:"TestList"`
	} `xml:"TestLists"`
	ResultSummary struct {
		Text     string `xml:",chardata"`
		Outcome  string `xml:"outcome,attr"`
		Counters struct {
			Text                string `xml:",chardata"`
			Total               string `xml:"total,attr"`
			Executed            string `xml:"executed,attr"`
			Passed              string `xml:"passed,attr"`
			Failed              string `xml:"failed,attr"`
			Error               string `xml:"error,attr"`
			Timeout             string `xml:"timeout,attr"`
			Aborted             string `xml:"aborted,attr"`
			Inconclusive        string `xml:"inconclusive,attr"`
			PassedButRunAborted string `xml:"passedButRunAborted,attr"`
			NotRunnable         string `xml:"notRunnable,attr"`
			NotExecuted         string `xml:"notExecuted,attr"`
			Disconnected        string `xml:"disconnected,attr"`
			Warning             string `xml:"warning,attr"`
			Completed           string `xml:"completed,attr"`
			InProgress          string `xml:"inProgress,attr"`
			Pending             string `xml:"pending,attr"`
		} `xml:"Counters"`
	} `xml:"ResultSummary"`
}
