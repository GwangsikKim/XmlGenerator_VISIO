import torch
import torch.nn.functional as F
import torch_geometric.nn as nn

class GCN(torch.nn.Module):
    def __init__(self, num_node_features, hidden_channels, num_classes):
        super(GCN, self).__init__()

        #GCNConv  GATConv  SAGEConv
        self.conv1 = nn.SAGEConv(num_node_features, hidden_channels)
        self.conv2 = nn.SAGEConv(hidden_channels, hidden_channels)
        self.conv3 = nn.SAGEConv(hidden_channels, hidden_channels)
        self.fc1 = nn.Linear(hidden_channels, hidden_channels)
        self.out = nn.Linear(hidden_channels, num_classes)

    def forward(self, data):
        x, edge_index = data.x, data.edge_index
        
        x = self.conv1(x, edge_index)
        x = F.relu(x)
        #x = F.dropout(x, training=self.training)
        
        x = self.conv2(x, edge_index)
        x = F.relu(x)
        
        x = self.conv3(x, edge_index)
        x = F.relu(x)        
        
        x = self.fc1(x)
        x = F.relu(x)
        
        x = self.out(x)
        
        return x