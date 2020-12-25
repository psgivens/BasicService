

import yaml

class EmitEvent :
    def __init__(self, event_name):
        self._event_name = event_name
    def __repr__(self):
        return "%s(event_name=%s)" % (
            self.__class__.__name__, self._event_name)


def emit_event_constructor(loader, node):
    value = loader.construct_scalar(node)
    return EmitEvent (value)
yaml.add_constructor(u'!EmitEvent', emit_event_constructor)


with open(r'bs.eventsmodel.yml') as file:
    # The FullLoader parameter handles the conversion from YAML
    # scalar values to Python the dictionary format
    fruits_list = yaml.load(file, Loader=yaml.FullLoader)

    print(fruits_list)
print ("Hello world")