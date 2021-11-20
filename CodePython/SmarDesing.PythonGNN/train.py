import random
import xml.etree.ElementTree as ET
import torch
import numpy as np
import matplotlib.pyplot as plt
from torch_geometric.data import Data
from torch_geometric.loader import DataLoader
from Model.model import GCN
from Model.model import GAT

#%%
device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')

TRAINSET_FILE_LIST = ["Dataset/5.xml", "Dataset/6.xml","Dataset/7.xml","Dataset/8.xml"
                      , "Dataset/13.xml","Dataset/14.xml","Dataset/15.xml","Dataset/16.xml"
                      , "Dataset/17.xml","Dataset/18.xml","Dataset/20.xml","Dataset/21.xml"]
VALIDATIONSET_FILE_LIST = ["Dataset/19.xml","Dataset/22.xml","Dataset/23.xml","Dataset/24.xml"]
TESTSET_FILE_LIST = ["Dataset/27.xml", "Dataset/38.xml","Dataset/43.xml", "Dataset/54.xml"]

NUM_NODE_FEATURES = 1
HIDDEN_CHANNELS = 8
NUM_CLASSES = 6

EPOCHS = 500
BATCH_SIZE = 1
LEARNING_RATE = 0.01
DECAY = 5e-3

#%%

def convert_symbol_to_node_feature(symbol_object):
    type_str = symbol_object.findtext("type")
    
    if type_str == "equipment_symbol":
        type_value = 0
    elif type_str == "instrument_symbol":
        type_value = 1
    elif type_str == "pipe_symbol":
        type_value = 2
    elif type_str == "text":
        type_value = 3
    elif type_str == "piping_line":
        type_value = 4
    elif type_str == "signal_line":
        type_value = 4
    else:
        type_value = -1
    
    return [type_value]
    
def convert_line_to_node_feature(line_object):
    
    type_str = line_object.findtext("type")
    
    if type_str == "equipment_symbol":
        type_value = 0
    elif type_str == "instrument_symbol":
        type_value = 1
    elif type_str == "pipe_symbol":
        type_value = 2
    elif type_str == "text":
        type_value = 3
    elif type_str == "piping_line":
        type_value = 4
    elif type_str == "signal_line":
        type_value = 4
    else:
        type_value = -1
    
    return [type_value]
    
def convert_to_label(obj):
    
    type_id = obj.findtext("type")
    
    if type_id == "equipment_symbol":
        return 0
    elif type_id == "instrument_symbol":
        return 1
    elif type_id == "pipe_symbol":
        return 2
    elif type_id == "text":
        return 3
    elif type_id == "piping_line":
        return 4
    elif type_id == "signal_line":
        return 5
    else:
        return -1
            
def find_node_index(node_list, node_id):
    
    for index, node in enumerate(node_list):
        if node[0] == node_id:
            return index
    
    return -1        
    
def read_xml(file_path):
    tree = ET.parse(file_path)
    root = tree.getroot()

    node_list = []
    
    symbol_objects = root.findall("symbol_object")
    
    for symbol_object in symbol_objects:
        features = convert_symbol_to_node_feature(symbol_object)
        
        label = convert_to_label(symbol_object)
        assert label != -1
            
        symbol_id = symbol_object.findtext("id")
        
        node_list.append((symbol_id, features, label))
    
    line_objects = root.findall("line_object")
    
    for line_object in line_objects:
        features = convert_line_to_node_feature(line_object)
        
        label = convert_to_label(line_object)
        assert label != -1
            
        line_id = line_object.findtext("id")
        
        node_list.append((line_id, features, label))

    random.shuffle(node_list)

    edge_list = []
    
    connection_objects = root.findall("connection_object")    
    
    for connection_object in connection_objects:
        if connection_object.find("connection") is not None:
            from_id = connection_object.find("connection").attrib["From"]
            to_id = connection_object.find("connection").attrib["To"]
            
            from_index = find_node_index(node_list, from_id)
            assert from_index != -1
            
            to_index = find_node_index(node_list, to_id)
            assert to_index != -1
            
            edge1 = [from_index, to_index]
            edge2 = [to_index, from_index]
            
            edge_list.append(edge1)
            edge_list.append(edge2)
            
    node_feature_list = [node[1] for node in node_list]
    node_labels = [node[2] for node in node_list]
    
    edge_index = torch.tensor(edge_list, dtype=torch.long)
    x = torch.tensor(node_feature_list, dtype=torch.float)
    y = torch.tensor(node_labels, dtype=torch.long)
    
    data = Data(x=x, edge_index=edge_index.t().contiguous(), y=y)

    return data

#%%
def train_loop(dataloader, model, loss_func, optimizer):
    model.train()

    loss_all = 0.0
    correct = 0
    size = 0
    
    for data in dataloader:
        data = data.to(device)
        size += len(data.batch)
        
        optimizer.zero_grad()
        output = model(data)
        label = data.y.to(device)
        
        loss = loss_func(output, label)        
        loss.backward()        
        optimizer.step()
        
        loss_all += loss.item()
        
        prediction = output.argmax(1) # 예측한 결과
        correct += (prediction == label).type(torch.float).sum().item() # 예측한 결과와 출력 데이터(정답)을 비교해 맞은 개수를 찾는다.
        
    accuracy = correct / size
    training_loss = loss_all / size
    
    return training_loss, accuracy

def test_loop(dataloader, model, loss_func):
    loss_all = 0
    correct = 0
    size = 0
    
    model.eval()

    with torch.no_grad(): # 미분을 수행하지 않음
        for data in dataloader:
            data = data.to(device)
            size += len(data.batch)
            
            output = model(data)
            label = data.y.to(device)
            
            loss = loss_func(output, label)        
            loss_all += loss.item()
            
            prediction = output.argmax(1) # 예측한 결과
            correct += (prediction == label).type(torch.float).sum().item()
    
    accuracy = correct / size
    test_loss = loss_all / size

    return test_loss, accuracy

def test_line_loop(dataloader, model, loss_func):
    loss_all = 0
    correct = 0
    size = 0
    
    model.eval()

    with torch.no_grad(): # 미분을 수행하지 않음
        for data in dataloader:
            data = data.to(device)           
            
            output = model(data)
            label = data.y.to(device)
            
            line_mask = data.x.flatten() == 4
            size += torch.sum((line_mask == True).int())
            
            loss = loss_func(output[line_mask], label[line_mask])        
            loss_all += loss.item()
            
            prediction = output.argmax(1) # 예측한 결과
            correct += (prediction[line_mask] == label[line_mask]).type(torch.float).sum().item()
    
    accuracy = correct / size
    test_loss = loss_all / size

    return test_loss.cpu().numpy(), accuracy.cpu().numpy()

#%% Train

trainset_data_list = [read_xml(file_path) for file_path in TRAINSET_FILE_LIST]
trainset_loader = DataLoader(trainset_data_list, batch_size=BATCH_SIZE, shuffle=True)

validationset_data_list = [read_xml(file_path) for file_path in VALIDATIONSET_FILE_LIST]
validationset_loader = DataLoader(validationset_data_list, batch_size=BATCH_SIZE, shuffle=True)
    
model = GCN(NUM_NODE_FEATURES, HIDDEN_CHANNELS, NUM_CLASSES).to(device)
#model = GAT(NUM_NODE_FEATURES, HIDDEN_CHANNELS, NUM_CLASSES).to(device)

optimizer = torch.optim.Adam(model.parameters(), lr=LEARNING_RATE, weight_decay=DECAY)
loss_function = torch.nn.CrossEntropyLoss()

train_loss_history = []
train_accuracy_history = []
validation_loss_history = []
validation_accuracy_history = []

model.train()

for epoch in range(EPOCHS):
    print("Epoch ", epoch+1)
    
    train_loss, train_accuracy = train_loop(trainset_loader, model, loss_function, optimizer)    
    train_loss_history.append(train_loss)
    train_accuracy_history.append(train_accuracy)
    
    validation_loss, validation_accuracy = test_loop(validationset_loader, model, loss_function)
    validation_loss_history.append(validation_loss)
    validation_accuracy_history.append(validation_accuracy)
  
    print("Training loss:", train_loss, "Training accuracy:", train_accuracy)
    print("Validation loss:", validation_loss, "Validation accuracy:", validation_accuracy)

#%% Plot

x = np.arange(1, len(train_loss_history)+1, dtype=int)

# Loss 그래프
plt.plot(x, train_loss_history, label="training")
plt.plot(x, validation_loss_history, label="validation")
plt.title('Loss')
plt.xlabel('Epoch')
plt.ylabel('Loss')
plt.legend()
plt.show()       

# Accuracy 그래프
plt.plot(x, train_accuracy_history, label="training")
plt.plot(x, validation_accuracy_history, label="validation")
plt.title('Accuracy')
plt.xlabel('Epoch')
plt.ylabel('Accuracy')
plt.legend()
plt.show()       

#%% Test
testset_data_list = [read_xml(file_path) for file_path in TESTSET_FILE_LIST]
testset_loader = DataLoader(testset_data_list, batch_size=BATCH_SIZE, shuffle=True)
   
test_loss, test_accuracy = test_loop(testset_loader, model, loss_function)
print("Test loss:", test_loss, "Test accuracy:", test_accuracy)

test_line_loss, test_line_accuracy = test_line_loop(testset_loader, model, loss_function)
print("Test line loss:", test_line_loss, "Test line accuracy:", test_line_accuracy)

#%%
"""
def plot_confusion_matrix(cm, target_names=None, cmap=None, normalize=True, labels=True, title='Confusion matrix'):
    accuracy = np.trace(cm) / float(np.sum(cm))
    misclass = 1 - accuracy

    if cmap is None:
        cmap = plt.get_cmap('Blues')

    if normalize:
        cm = cm.astype('float') / cm.sum(axis=1)[:, np.newaxis]
        
    plt.figure(figsize=(8, 6))
    plt.imshow(cm, interpolation='nearest', cmap=cmap)
    plt.title(title)
    plt.colorbar()

    thresh = cm.max() / 1.5 if normalize else cm.max() / 2
    
    if target_names is not None:
        tick_marks = np.arange(len(target_names))
        plt.xticks(tick_marks, target_names)
        plt.yticks(tick_marks, target_names)
    
    if labels:
        for i, j in itertools.product(range(cm.shape[0]), range(cm.shape[1])):
            if normalize:
                plt.text(j, i, "{:0.4f}".format(cm[i, j]),
                         horizontalalignment="center",
                         color="white" if cm[i, j] > thresh else "black")
            else:
                plt.text(j, i, "{:,}".format(cm[i, j]),
                         horizontalalignment="center",
                         color="white" if cm[i, j] > thresh else "black")

    plt.tight_layout()
    plt.ylabel('True label')
    plt.xlabel('Predicted label\naccuracy={:0.4f}; misclass={:0.4f}'.format(accuracy, misclass))
    plt.show()

plot_confusion_matrix()
"""