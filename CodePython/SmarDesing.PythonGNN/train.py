import random
import xml.etree.ElementTree as ET
import torch
import numpy as np
import matplotlib.pyplot as plt
from torch_geometric.data import Data
from torch_geometric.loader import DataLoader
from Model.model import GCN

#%%
device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')

TRAINSET_FILE_LIST = ["Dataset/13.xml", "Dataset/6.xml", "Dataset/43.xml", "Dataset/38.xml", "Dataset/17.xml",
                      "Dataset/23.xml", "Dataset/5.xml", "Dataset/8.xml", "Dataset/16.xml", "Dataset/20.xml",
                      "Dataset/21.xml", "Dataset/24.xml"]
VALIDATIONSET_FILE_LIST = ["Dataset/15.xml", "Dataset/14.xml", "Dataset/7.xml", "Dataset/19.xml"]
TESTSET_FILE_LIST = ["Dataset/18.xml", "Dataset/54.xml", "Dataset/22.xml", "Dataset/27.xml"]

NUM_NODE_FEATURES = 1
HIDDEN_CHANNELS = 128
NUM_CLASSES = 5

EPOCHS = 200
BATCH_SIZE = 1
LEARNING_RATE = 0.01
DECAY = 5e-3

#%%

def convert_symbol_to_node_feature(symbol_object):
    type_str = symbol_object.findtext("type")
    
    if type_str == "equipment_symbol":
        type_value = -1
    elif type_str == "instrument_symbol":
        type_value = -0.5
    elif type_str == "pipe_symbol":
        type_value = 0.5
    #elif type_str == "text":
    #    type_value = 0.5
    elif type_str == "piping_line":
        type_value = 1
    elif type_str == "signal_line":
        type_value = 1
    else:
        assert False
    
    return [type_value]
    
def convert_line_to_node_feature(line_object):
    
    type_str = line_object.findtext("type")
    
    if type_str == "equipment_symbol":
        type_value = -1
    elif type_str == "instrument_symbol":
        type_value = -0.5
    elif type_str == "pipe_symbol":
        type_value = 0.5
    #elif type_str == "text":
    #    type_value = 0.5
    elif type_str == "piping_line":
        type_value = 1
    elif type_str == "signal_line":
        type_value = 1
    else:
        assert False
    
    return [type_value]
    
def convert_to_label(obj):
    
    type_id = obj.findtext("type")
    
    if type_id == "equipment_symbol":
        return 0
    elif type_id == "instrument_symbol":
        return 1
    elif type_id == "pipe_symbol":
        return 2
    #elif type_id == "text":
    #    return 0
    elif type_id == "piping_line":
        return 3
    elif type_id == "signal_line":
        return 4
    else:
        assert False
            
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
        type_str = symbol_object.findtext("type")
        if type_str == "text":
            continue
        
        features = convert_symbol_to_node_feature(symbol_object)
        label = convert_to_label(symbol_object)
        symbol_id = symbol_object.findtext("id")
        
        node_list.append((symbol_id, features, label))
    
    line_objects = root.findall("line_object")
    
    for line_object in line_objects:
        type_str = line_object.findtext("type")
        if type_str == "text":
            continue
        
        features = convert_line_to_node_feature(line_object)
        label = convert_to_label(line_object)
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
            if from_index == -1:
                continue
            
            to_index = find_node_index(node_list, to_id)
            if to_index == -1:
                continue
            
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
    print(data.size())
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
    
    prediction_list = []
    label_list = []
    
    model.eval()

    with torch.no_grad(): # 미분을 수행하지 않음
        for data in dataloader:
            data = data.to(device)
            size += len(data.batch)
            
            output = model(data)
            label = data.y.to(device)
            label_list.extend(label.cpu())
            
            loss = loss_func(output, label)        
            loss_all += loss.item()
            
            prediction = output.argmax(1) # 예측한 결과
            prediction_list.extend(prediction.cpu())
            
            correct += (prediction == label).type(torch.float).sum().item()
            
    
    accuracy = correct / size
    test_loss = loss_all / size

    return test_loss, accuracy, label_list, prediction_list

#%% Train

trainset_data_list = [read_xml(file_path) for file_path in TRAINSET_FILE_LIST]
trainset_loader = DataLoader(trainset_data_list, batch_size=BATCH_SIZE, shuffle=True)

validationset_data_list = [read_xml(file_path) for file_path in VALIDATIONSET_FILE_LIST]
validationset_loader = DataLoader(validationset_data_list, batch_size=BATCH_SIZE, shuffle=True)
    
model = GCN(NUM_NODE_FEATURES, HIDDEN_CHANNELS, NUM_CLASSES).to(device)
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
    
    validation_loss, validation_accuracy, _, _ = test_loop(validationset_loader, model, loss_function)
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
   
test_loss, test_accuracy, label_list, prediction_list = test_loop(testset_loader, model, loss_function)
print("Test loss:", test_loss, "Test accuracy:", test_accuracy)

#%% confusion_matrix  
from sklearn.metrics import confusion_matrix
  
def plot_confusion_matrix(y_true, y_pred, classes,
                          normalize=False,
                          title=None,
                          cmap=plt.cm.Blues):
    if not title:
        if normalize:
            title = 'Normalized confusion matrix'
        else:
            title = 'Confusion matrix, without normalization'

    cm = confusion_matrix(y_true, y_pred)
    if normalize:
        cm = cm.astype('float') / cm.sum(axis=1)[:, np.newaxis]
        print("Normalized confusion matrix")
    else:
        print('Confusion matrix, without normalization')

    print(cm)

    fig, ax = plt.subplots()
    im = ax.imshow(cm, interpolation='nearest', cmap=cmap)
    ax.figure.colorbar(im, ax=ax)
    
    ax.set(xticks=np.arange(cm.shape[1]),
           yticks=np.arange(cm.shape[0]),
           xticklabels=classes, yticklabels=classes,
           title=title,
           ylabel='True label',
           xlabel='Predicted label')

    plt.setp(ax.get_xticklabels(), rotation=45, ha="right",
             rotation_mode="anchor")

    fmt = '.2f' if normalize else 'd'
    thresh = cm.max() / 2.
    for i in range(cm.shape[0]):
        for j in range(cm.shape[1]):
            ax.text(j, i, format(cm[i, j], fmt),
                    ha="center", va="center",
                    color="white" if cm[i, j] > thresh else "black")
    fig.tight_layout()
    return ax

#%%
label = ["equipment", "instrument", "piping", "piping_line","signal_line"]

plot_confusion_matrix(label_list, prediction_list, classes=label,
                      title='Confusion matrix')