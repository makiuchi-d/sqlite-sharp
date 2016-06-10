
ifndef NUNIT_FW_PATH
	NUNIT_FW_PATH = $(shell dirname `find /usr/lib -name nunit.framework.dll|head -1`)
endif

CSC = mcs
CSCFLAGS = -r:$(NUNIT_FW_PATH)/nunit.framework.dll -target:library

TEST = MONO_PATH=$(NUNIT_FW_PATH) nunit-console
TESTFLAGS = -domain:None

SOURCES = ../SqliteSharp/*.cs
TEST_SOURCES = \
	DataRowTest.cs \
	StatementTest.cs \


.PHONY: test clean

test: $(TEST_SOURCES:%.cs=%.dll)
	$(TEST) $(TESTFLAGS) $^

clean:
	$(RM) $(TEST_SOURCES:%.cs=%.dll) TestResult.xml

%.dll: %.cs $(SOURCES)
	$(CSC) $(CSCFLAGS) -out:$@ $^

