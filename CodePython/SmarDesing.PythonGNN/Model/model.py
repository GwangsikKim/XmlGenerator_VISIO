import torch
import torch.nn.functional as F
import torch_geometric.nn as nn

class GCN(torch.nn.Module):
    def __init__(self, num_node_features, hidden_channels, num_classes):
        super(GCN, self).__init__()

        self.conv1 = nn.GATConv(num_node_features, hidden_channels)
        self.conv2 = nn.GATConv(hidden_channels, hidden_channels)
        self.out = nn.Linear(hidden_channels, num_classes)

    def forward(self, data):
        x, edge_index = data.x, data.edge_index
        
        x = self.conv1(x, edge_index)
        x = F.relu(x)
        #x = F.dropout(x, training=self.training)
        
        x = self.conv2(x, edge_index)
        x = F.relu(x)
        #x = F.dropout(x, training=self.training)
        
        x = self.out(x)

        return x

#%%
class GAT(torch.nn.Module):
    def __init__(self, num_node_features, hidden_channels, num_classes):
        super(GAT, self).__init__()
        self.gat1 = nn.GATConv(num_node_features, hidden_channels)
        self.gat2 = nn.GATConv(hidden_channels, hidden_channels)
        self.out = nn.Linear(hidden_channels, num_classes)

    def forward(self, data):
        x, edge_index = data.x, data.edge_index
        
        x= self.gat1(x, edge_index)
        x= F.relu(x)
        #x= F.dropout(x, p=0.5, training= self.training)
        
        x= self.gat2(x, edge_index)
        
        x = self.out(x)
        return x
        
#%%
class MLP(torch.nn.Module):
    def __init__ (self, num_node_features, hidden_channels, num_classes):
        super(MLP, self).__init__()
        self.lin1 = nn.Linear(num_node_features, hidden_channels)
        self.lin2 = nn.Linear(hidden_channels, num_classes)

    def forward(self, data):
        x = data.x
        
        x= self.lin1(x)
        x= F.relu(x)
        #x= F.dropout(x, p=0.5, training =self.training)
     
        x= self.lin2(x)
        return x