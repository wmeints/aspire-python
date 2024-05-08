import logging
from opentelemetry import trace

tracer = trace.get_tracer(__name__)

# Make sure you have the proper configuration for the application.
# By default the root logger only outputs warning or higher. You'll want everything at root level.
# You can limit the amount of logging on lower levels later on.
logging.basicConfig()
logging.root.setLevel(logging.NOTSET)

# You can use tracing to track activities in the application using spans.
# Depending on the framework, like django or flask, you may already have a span running when handling an incoming request.
# For this console application you'll need to set the root span yourself.
with tracer.start_as_current_span("main"):
    logger = logging.getLogger(__name__)
    logger.error("Hello there!")
